using System;
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

        /// <summary>
        /// Attempts to delete the directory and retries if unable to
        /// </summary>
        /// <returns>False if the directory exists</returns>
        public static async Task<bool> DeleteDirectory(string path, bool recursive, UploadTaskReport.StepResult result)
        {
            const int delayBetweenRetriesMS = 5000;
            const int maxRetryAttempts = 3; // max 15 seconds
            
            int attempts = 0;
            while (++attempts <= maxRetryAttempts)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        return true; // true because doesn't exist
                    }
                    
                    Directory.Delete(path, recursive);
                }
                catch (DirectoryNotFoundException e)
                {
                    result.AddException(e);
                    result.AddError($"Failed to delete directory. Your folder path is likely too long. Try changing the cache directory in preferences: '{path}'");
                    return false;
                }
                catch (IOException e)
                {
                    // Likely: System.IO.IOException: The process cannot access the file
                    // Could be because something locked the build folder so wait a little bit before retrying
                    result.AddException(e);
                    result.AddWarning($"Got an IO exception deleting directory. Waiting a while before retrying: '{path}'");
                    await Task.Delay(delayBetweenRetriesMS);
                }
                catch (Exception e)
                {
                    result.AddException(e);
                    result.AddError($"Got an Exception deleting directory! '{path}'");
                    break;
                }
            }
            
            return !Directory.Exists(path);
        }
    }
}