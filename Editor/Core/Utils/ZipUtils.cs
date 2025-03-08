using System.Threading.Tasks;
#if UNITY_2021_1_OR_NEWER
using System.IO.Compression;
#else
// NOTE: If the below package is red then you need to add the package 'com.unity.sharp-zip-lib' to your project
using Unity.SharpZipLib.Zip;
using System;
using System.IO;
#endif

namespace Wireframe
{
    /// <summary>
    /// The Build Uploader zips and unzips files according to what needs what format
    /// This class acts as the Util that all systems can use to zip and unzip content
    /// 
    /// Unity 2022 we can use ZipFile from C#
    /// Below that requires adding the package com.unity.sharp-zip-lib 
    /// </summary>
    internal static class ZipUtils
    {
        public static async Task<bool> Zip(string filePath, string zippedfilePath)
        {
#if UNITY_2021_1_OR_NEWER
            return await Task.Run(() =>
            {
                try
                {
                    ZipFile.CreateFromDirectory(filePath, zippedfilePath);
                    return true;
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Failed to zip file: {e.Message}");
                    return false;
                }
            });
#else
            // Note: This is VERY slow. Don't know why!
            try{
                string[] filenames = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
                using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zippedfilePath)))
                {
                    zipStream.SetLevel(9);
                    for (var i = 0; i < filenames.Length; i++)
                    {
                        var file = filenames[i];
                        var estimatedFileName = file.Substring(filePath.Length);
                        var entry = new ZipEntry(estimatedFileName);
                        entry.DateTime = DateTime.Now;
                        zipStream.PutNextEntry(entry);

                        byte[] bytes = await File.ReadAllBytesAsync(file);
                        zipStream.Write(bytes, 0, bytes.Length);
                    }

                    zipStream.Finish();
                    zipStream.Close();
                }
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to zip file: {e.Message}");
                return false;
            }
            
            // try
            // {
            //     FastZip fastZip = new FastZip();
            //     fastZip.CreateEmptyDirectories = true;
            //     fastZip.CreateZip(filePath, zippedfilePath, true, "", "");
            //     return true;
            // }
            // catch (System.Exception e)
            // {
            //     UnityEngine.Debug.LogError($"Failed to zip file: {e.Message}");
            //     return false;
            // }
#endif
        }
        
        public static void UnZip(string filePath, string unzippedfilePath)
        {
#if UNITY_2021_1_OR_NEWER
            ZipFile.ExtractToDirectory(filePath, unzippedfilePath);
#else
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(filePath, unzippedfilePath, null);
#endif
        }
    }
}