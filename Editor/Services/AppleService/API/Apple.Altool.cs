using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    /// <summary>
    /// Wrapper around `xcrun altool --upload-app` for binary IPA uploads to TestFlight.
    ///
    /// altool is part of Xcode's command line tools and is macOS-only. The App Store
    /// Connect REST API does not expose a public binary upload endpoint, so this is
    /// the supported path for getting an IPA into TestFlight from scripts.
    ///
    /// Reference:
    ///   altool --help (Xcode 14+)
    ///   https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/
    /// </summary>
    public static partial class Apple
    {
        // altool serialises uploads internally on the Apple side; running multiple
        // instances against the same key has historically caused intermittent failures.
        // Match the Itchio pattern and serialise on our side too.
        private static readonly SemaphoreSlim s_altoolLock = new SemaphoreSlim(1);

        /// <summary>
        /// Uploads an .ipa to App Store Connect via xcrun altool.
        ///
        /// The API Key's .p8 file is exposed to altool by setting API_PRIVATE_KEYS_DIR
        /// to its parent directory for the duration of the call. This avoids requiring
        /// users to drop the key into ~/.appstoreconnect/private_keys.
        /// </summary>
        public static async Task<AppleAltoolUploadResponse> UploadIPA(
            string ipaPath,
            ApplePlatform platform,
            AppleConfig.AppleApiKey apiKey,
            UploadTaskReport.StepResult result)
        {
            if (!IsRunningOnMac)
            {
                result?.SetFailed("Apple uploads via xcrun altool require macOS. The Unity Editor is currently running on " + UnityEngine.Application.platform + ".");
                return new AppleAltoolUploadResponse(false);
            }

            if (string.IsNullOrEmpty(ipaPath) || !File.Exists(ipaPath))
            {
                result?.SetFailed($"IPA not found at: {ipaPath}");
                return new AppleAltoolUploadResponse(false);
            }

            if (apiKey == null || string.IsNullOrEmpty(apiKey.KeyID) || string.IsNullOrEmpty(apiKey.IssuerID))
            {
                result?.SetFailed("Apple API Key is missing Key ID or Issuer ID.");
                return new AppleAltoolUploadResponse(false);
            }

            string p8Path = apiKey.PrivateKeyPath;
            if (string.IsNullOrEmpty(p8Path) || !File.Exists(p8Path))
            {
                result?.SetFailed($"Apple API Key '{apiKey.Name}' .p8 file not found at: {p8Path}");
                return new AppleAltoolUploadResponse(false);
            }

            string keysDir = Path.GetDirectoryName(p8Path);
            string expectedFileName = $"AuthKey_{apiKey.KeyID}.p8";
            string actualFileName = Path.GetFileName(p8Path);
            if (!string.Equals(actualFileName, expectedFileName, StringComparison.Ordinal))
            {
                // altool resolves the key by filename convention. If the user pointed at a
                // renamed file, surface the issue early rather than getting a confusing
                // "key not found" from altool.
                result?.SetFailed(
                    $"Apple API Key file must be named '{expectedFileName}' for altool to find it. " +
                    $"Got: '{actualFileName}'. Rename the file or re-export it from App Store Connect.");
                return new AppleAltoolUploadResponse(false);
            }

            await s_altoolLock.WaitAsync();

            Process process = null;
            try
            {
                process = new Process();
                process.StartInfo.FileName = "/usr/bin/xcrun";
                process.StartInfo.Arguments = BuildAltoolArguments(ipaPath, platform, apiKey);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.EnvironmentVariables["API_PRIVATE_KEYS_DIR"] = keysDir;

                result?.AddLog($"Running: /usr/bin/xcrun {process.StartInfo.Arguments}");

                try
                {
                    if (!process.Start())
                    {
                        result?.SetFailed("Could not start xcrun altool. Is Xcode installed?");
                        return new AppleAltoolUploadResponse(false);
                    }
                }
                catch (Exception e)
                {
                    result?.AddException(e);
                    result?.SetFailed("Could not start xcrun altool: " + e.Message);
                    return new AppleAltoolUploadResponse(false);
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
                Task<string> stderrTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(stdoutTask, stderrTask);

                try
                {
                    process.WaitForExit();
                }
                catch (Exception e)
                {
                    result?.AddException(e);
                }

                stopwatch.Stop();

                string stdout = stdoutTask.Result ?? "";
                string stderr = stderrTask.Result ?? "";
                int exitCode = process.ExitCode;

                StringBuilder combined = new StringBuilder();
                if (stdout.Length > 0) combined.AppendLine(stdout);
                if (stderr.Length > 0) combined.AppendLine(stderr);
                string combinedText = combined.ToString();

                result?.AddLog($"altool exited with code {exitCode} after {stopwatch.ElapsedMilliseconds}ms");
                if (combinedText.Length > 0)
                {
                    result?.AddLog(combinedText);
                }

                if (exitCode != 0)
                {
                    result?.SetFailed($"altool upload failed (exit {exitCode}). See logs for details.");
                    return new AppleAltoolUploadResponse(false);
                }

                return new AppleAltoolUploadResponse(true);
            }
            catch (Exception e)
            {
                result?.AddException(e);
                result?.SetFailed("Could not upload IPA via altool: " + e.Message);
                return new AppleAltoolUploadResponse(false);
            }
            finally
            {
                try { process?.Close(); } catch { /* swallow */ }
                s_altoolLock.Release();
            }
        }

        private static string BuildAltoolArguments(
            string ipaPath,
            ApplePlatform platform,
            AppleConfig.AppleApiKey apiKey)
        {
            // xcrun altool --upload-app -t <platform> -f <ipa> --api-key <id> --api-issuer <issuer> --output-format json
            string altoolType = PlatformToAltoolType(platform);
            return $"altool --upload-app " +
                   $"-t {altoolType} " +
                   $"-f \"{ipaPath}\" " +
                   $"--api-key {apiKey.KeyID} " +
                   $"--api-issuer {apiKey.IssuerID} " +
                   $"--output-format json";
        }
    }
}
