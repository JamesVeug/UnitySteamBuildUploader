#if BUILD_UPLOADER_GRAPHTOOLKIT
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Downloads the Discord brand mark once and caches it on disk as a Texture2D for reuse.
    ///
    /// Uses RequestWrapper (project convention — never UnityWebRequest/HttpClient directly). The cached PNG lives
    /// under the per-project BuildUploader folder so it isn't re-fetched every domain reload.
    ///
    /// NOTE: Graph Toolkit 0.4-exp.2 exposes no public API to assign a custom icon to a node header, so this texture
    /// can't yet be bound as DiscordMessageBlock's node icon. It's produced/cached here so it's ready the moment the
    /// package adds an icon hook (or for use in any custom inspector). See NodeGraph-Design.md "API limitations".
    /// </summary>
    public static class DiscordIconLoader
    {
        // Texture2D.LoadImage only decodes PNG/JPG (not SVG). This points at Wikimedia's thumb endpoint, which
        // rasterises to PNG. It could NOT be verified from the build environment — confirm it resolves to a PNG,
        // or swap in your own raster URL.
        public const string IconUrl =
            "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c9/Discord_Logo_2021.svg/240px-Discord_Logo_2021.svg.png";

        private static Texture2D s_cached;

        private static string CacheFilePath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "BuildUploader", "Cache", "discord_icon.png");

        /// <summary>Returns the cached icon, loading from disk if present. Null until <see cref="DownloadAsync"/> runs.</summary>
        public static Texture2D Get()
        {
            if (s_cached != null)
            {
                return s_cached;
            }

            string path = CacheFilePath;
            if (File.Exists(path))
            {
                s_cached = LoadFromBytes(File.ReadAllBytes(path));
            }

            return s_cached;
        }

        [MenuItem("Window/Build Uploader/Graph/Download Discord Icon")]
        private static void DownloadMenu() => _ = DownloadAsync();

        /// <summary>Fetches the icon, decodes it to a Texture2D, and caches the PNG to disk. Returns null on failure.</summary>
        public static async Task<Texture2D> DownloadAsync(string url = IconUrl)
        {
            using (RequestWrapper www = RequestWrapper.Get(url))
            {
                RequestResult response = await www.SendAsync(null);
                if (!response.IsSuccessful || response.Bytes == null)
                {
                    Debug.LogError($"[Build Uploader Graph] Failed to download Discord icon from {url}: {response.Data}");
                    return null;
                }

                Texture2D texture = LoadFromBytes(response.Bytes);
                if (texture == null)
                {
                    Debug.LogError("[Build Uploader Graph] Downloaded data was not a valid PNG/JPG image.");
                    return null;
                }

                string path = CacheFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
                File.WriteAllBytes(path, texture.EncodeToPNG());

                s_cached = texture;
                Debug.Log($"[Build Uploader Graph] Discord icon cached to {path}");
                return texture;
            }
        }

        private static Texture2D LoadFromBytes(byte[] bytes)
        {
            Texture2D texture = new Texture2D(2, 2);
            return texture.LoadImage(bytes) ? texture : null;
        }
    }
}
#endif
