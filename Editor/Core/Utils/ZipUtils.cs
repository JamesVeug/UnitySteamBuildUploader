using System.IO;
using System.Threading.Tasks;
#if UNITY_2021_1_OR_NEWER
using System.IO.Compression;
#else
// NOTE: If the below package is red then you need to add the package 'com.unity.sharp-zip-lib' to your project
using Unity.SharpZipLib.Zip;
using System;
#endif

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Wireframe
{
    /// <summary>
    /// The Build Uploader zips and unzips files according to what needs what format
    /// This class acts as the Util that all systems can use to zip and unzip content
    /// 
    /// Unity 2022 we can use ZipFile from C#
    /// Below that requires adding the package com.unity.sharp-zip-lib 
    /// </summary>
    public static class ZipUtils
    {
        public static async Task<bool> Zip(string filePath, string zippedfilePath, BuildTaskReport.StepResult result)
        {
#if UNITY_2021_1_OR_NEWER
            return await Task.Run(() =>
            {
                try
                {
                    if (zippedfilePath.StartsWith(filePath))
                    {
                        // Cannot zip a file into itself
                        // So zip it to a new folder and then move it
                        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(zippedfilePath));
                        ZipFile.CreateFromDirectory(filePath, tempPath);
                        File.Move(tempPath, zippedfilePath);
                        return true;
                    }

                    ZipFile.CreateFromDirectory(filePath, zippedfilePath);
                    return true;
                }
                catch (System.Exception e)
                {
                    result.AddException(e);
                    result.SetFailed($"Failed to zip file: {e.Message}");
                    return false;
                }
            });
#else
            // Note: This is VERY slow. Don't know why!
            try{
                bool zippingIntoSamePath = zippedfilePath.StartsWith(filePath);
                string zipPath = zippedfilePath;
                if (zippingIntoSamePath)
                {
                    zipPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(zippedfilePath));
                }
                
                string[] filenames = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
                using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipPath)))
                {
                    zipStream.SetLevel(9);
                    for (var i = 0; i < filenames.Length; i++)
                    {
                        var file = filenames[i];
                        var estimatedFileName = file.Substring(filePath.Length);
                        var entry = new ZipEntry(estimatedFileName);
                        entry.DateTime = DateTime.Now;
                        zipStream.PutNextEntry(entry);

                        byte[] bytes = File.ReadAllBytes(file);
                        zipStream.Write(bytes, 0, bytes.Length);
                    }

                    zipStream.Finish();
                    zipStream.Close();
                }
                
                if (zippingIntoSamePath)
                {
                    File.Move(zipPath, zippedfilePath);
                }
                return true;
            }
            catch (System.Exception e)
            {
                result.AddException(e);
                result.SetFailed($"Failed to zip file: {e.Message}");
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