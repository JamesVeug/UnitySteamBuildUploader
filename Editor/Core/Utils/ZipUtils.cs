#if UNITY_2021_1_OR_NEWER
using System.IO.Compression;
#else
// NOTE: If the below package is red then you need to add the package 'com.unity.sharp-zip-lib' to your project
using Unity.SharpZipLib.Zip;
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