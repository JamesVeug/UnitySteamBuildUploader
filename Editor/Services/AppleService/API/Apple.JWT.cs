using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wireframe
{
    /// <summary>
    /// JWT generation for the App Store Connect API.
    ///
    /// App Store Connect requires ES256 (ECDSA with curve P-256 + SHA-256) JWTs.
    /// Reference: https://developer.apple.com/documentation/appstoreconnectapi/creating_api_keys_for_app_store_connect_api
    /// </summary>
    public static partial class Apple
    {
        private const string JWT_AUDIENCE = "appstoreconnect-v1";

        // Apple's docs cap token lifetime at 20 minutes. We refresh well before that.
        private const int JWT_LIFETIME_SECONDS = 15 * 60;
        private const int JWT_REFRESH_THRESHOLD_SECONDS = 60;

        private struct CachedJWT
        {
            public string Token;
            public long ExpiresAtUnix;
        }

        private static readonly Dictionary<string, CachedJWT> s_jwtCache = new Dictionary<string, CachedJWT>();

        /// <summary>
        /// Returns a JWT for the given API key, generating (and caching) a new one if
        /// the current cached token is missing or near expiry. Returns null and logs to
        /// stepResult if the .p8 file cannot be read or parsed.
        /// </summary>
        public static string GetJWT(AppleConfig.AppleApiKey apiKey, UploadTaskReport.StepResult result = null)
        {
            if (apiKey == null)
            {
                result?.AddError("Apple API Key is null.");
                return null;
            }

            if (string.IsNullOrEmpty(apiKey.IssuerID))
            {
                result?.AddError($"Apple API Key '{apiKey.Name}' has no Issuer ID. Set it in Project Settings.");
                return null;
            }

            if (string.IsNullOrEmpty(apiKey.KeyID))
            {
                result?.AddError($"Apple API Key '{apiKey.Name}' has no Key ID. Set it in Project Settings.");
                return null;
            }

            string cacheKey = apiKey.IssuerID + ":" + apiKey.KeyID;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (s_jwtCache.TryGetValue(cacheKey, out CachedJWT cached) &&
                cached.ExpiresAtUnix - now > JWT_REFRESH_THRESHOLD_SECONDS)
            {
                return cached.Token;
            }

            string p8Path = apiKey.PrivateKeyPath;
            if (string.IsNullOrEmpty(p8Path))
            {
                result?.AddError($"Apple API Key '{apiKey.Name}' has no .p8 file path set. Set it in Preferences.");
                return null;
            }

            if (!File.Exists(p8Path))
            {
                result?.AddError($"Apple API Key '{apiKey.Name}' .p8 file not found at: {p8Path}");
                return null;
            }

            try
            {
                string pem = File.ReadAllText(p8Path);
                long exp = now + JWT_LIFETIME_SECONDS;
                string token = BuildJWT(pem, apiKey.IssuerID, apiKey.KeyID, exp);
                s_jwtCache[cacheKey] = new CachedJWT { Token = token, ExpiresAtUnix = exp };
                return token;
            }
            catch (Exception e)
            {
                result?.AddException(e);
                result?.AddError($"Failed to sign Apple JWT for key '{apiKey.Name}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Invalidates the cached JWT for an API key. Call this if a request returns 401.
        /// </summary>
        internal static void InvalidateJWT(AppleConfig.AppleApiKey apiKey)
        {
            if (apiKey == null) return;
            string cacheKey = apiKey.IssuerID + ":" + apiKey.KeyID;
            s_jwtCache.Remove(cacheKey);
        }

        private static string BuildJWT(string pem, string issuerId, string keyId, long expiresAtUnix)
        {
            // Header: ES256, kid = KeyID, typ = JWT
            string headerJson = "{\"alg\":\"ES256\",\"kid\":\"" + keyId + "\",\"typ\":\"JWT\"}";
            string payloadJson = "{\"iss\":\"" + issuerId +
                                 "\",\"exp\":" + expiresAtUnix +
                                 ",\"aud\":\"" + JWT_AUDIENCE + "\"}";

            string headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            string payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
            string signingInput = headerB64 + "." + payloadB64;

            byte[] signature = SignES256(pem, Encoding.UTF8.GetBytes(signingInput));
            string signatureB64 = Base64UrlEncode(signature);

            return signingInput + "." + signatureB64;
        }

        private static byte[] SignES256(string pem, byte[] data)
        {
            byte[] pkcs8 = DecodePemToPkcs8(pem);

            using (ECDsa ecdsa = ECDsa.Create())
            {
                ecdsa.ImportPkcs8PrivateKey(pkcs8, out _);

                // .NET's SignData defaults to DER-encoded ASN.1 signatures on older runtimes.
                // JWS ES256 mandates the raw r||s concatenation (IEEE P1363). We compute the
                // hash explicitly and convert any DER signature we get back to the JOSE shape.
                byte[] hash;
                using (SHA256 sha = SHA256.Create())
                {
                    hash = sha.ComputeHash(data);
                }

                byte[] raw = ecdsa.SignHash(hash);

                // ECDsa.SignHash on P-256 in JOSE-compatible runtimes already returns 64
                // bytes of r||s. If the runtime returned DER instead, convert.
                if (raw.Length == 64)
                {
                    return raw;
                }
                return DerSignatureToJose(raw, 32);
            }
        }

        /// <summary>
        /// Decodes a PEM-wrapped PKCS#8 private key to its raw byte payload.
        /// </summary>
        private static byte[] DecodePemToPkcs8(string pem)
        {
            if (string.IsNullOrEmpty(pem))
            {
                throw new ArgumentException("PEM content is empty.");
            }

            const string header = "-----BEGIN PRIVATE KEY-----";
            const string footer = "-----END PRIVATE KEY-----";

            int start = pem.IndexOf(header, StringComparison.Ordinal);
            int end = pem.IndexOf(footer, StringComparison.Ordinal);
            if (start < 0 || end < 0 || end <= start)
            {
                throw new FormatException(
                    "Apple .p8 file is not in expected PKCS#8 PEM format (missing BEGIN/END PRIVATE KEY markers).");
            }

            start += header.Length;
            string base64 = pem.Substring(start, end - start);

            // Strip whitespace from inside the PEM body
            StringBuilder sb = new StringBuilder(base64.Length);
            foreach (char c in base64)
            {
                if (c != '\r' && c != '\n' && c != ' ' && c != '\t')
                {
                    sb.Append(c);
                }
            }

            return Convert.FromBase64String(sb.ToString());
        }

        /// <summary>
        /// Converts a DER-encoded ECDSA signature (SEQUENCE of two INTEGERs r and s) into
        /// the fixed-field r||s concatenation that JWS ES256 requires.
        /// </summary>
        private static byte[] DerSignatureToJose(byte[] der, int fieldSize)
        {
            if (der == null || der.Length < 8 || der[0] != 0x30)
            {
                throw new FormatException("ECDSA signature is not in expected DER format.");
            }

            int offset = 2;
            if ((der[1] & 0x80) != 0)
            {
                offset += der[1] & 0x7f;
            }

            if (der[offset] != 0x02)
            {
                throw new FormatException("ECDSA DER signature missing INTEGER tag for r.");
            }
            int rLen = der[offset + 1];
            int rOff = offset + 2;
            offset = rOff + rLen;

            if (der[offset] != 0x02)
            {
                throw new FormatException("ECDSA DER signature missing INTEGER tag for s.");
            }
            int sLen = der[offset + 1];
            int sOff = offset + 2;

            byte[] result = new byte[fieldSize * 2];
            CopyIntegerToFixedField(der, rOff, rLen, result, 0, fieldSize);
            CopyIntegerToFixedField(der, sOff, sLen, result, fieldSize, fieldSize);
            return result;
        }

        private static void CopyIntegerToFixedField(byte[] src, int srcOffset, int srcLen,
            byte[] dst, int dstOffset, int fieldSize)
        {
            // DER INTEGERs may have a leading 0x00 to disambiguate the sign bit; skip it.
            while (srcLen > 0 && src[srcOffset] == 0)
            {
                srcOffset++;
                srcLen--;
            }

            if (srcLen > fieldSize)
            {
                throw new FormatException("ECDSA integer exceeds field size.");
            }

            int pad = fieldSize - srcLen;
            for (int i = 0; i < pad; i++)
            {
                dst[dstOffset + i] = 0;
            }
            Buffer.BlockCopy(src, srcOffset, dst, dstOffset + pad, srcLen);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            string b64 = Convert.ToBase64String(bytes);
            return b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
