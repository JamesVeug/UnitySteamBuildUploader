using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    public static class IOUtils
    {
        public static Task WriteAllTextAsync(string path, string content)
        {
#if UNITY_2021_1_OR_NEWER
            return File.WriteAllTextAsync(path, content);
#else
            return Task.Run(() =>
            {
                File.WriteAllText(path, content);
            });
#endif
        }

        public static Task WriteAllBytesAsync(string path, byte[] content)
        {
#if UNITY_2021_2_OR_NEWER
            return File.WriteAllBytesAsync(path, content);
#else
            return Task.Run(() =>
            {
                File.WriteAllBytes(path, content);
            });
#endif
        }

        public static Task<byte[]> ReadAllBytesAsync(string path)
        {
#if UNITY_2021_2_OR_NEWER
            return File.ReadAllBytesAsync(path);
#else
            return Task.Run(() =>
            {
                return File.ReadAllBytes(path);
            });
#endif
        }
    }
}