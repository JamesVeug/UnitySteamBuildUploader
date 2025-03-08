using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    internal class BuildTaskStep_CacheSources : ABuildTask_Step
    {
        public override string Name => "Cache Sources";
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = CacheBuildConfigAtIndex(buildTask, j);
                tasks.Add(task);
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Task<bool> task = tasks[j];
                    if (!task.IsCompleted)
                    {
                        done = false;
                    }
                    else
                    {
                        allSuccessful &= task.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / tasks.Count;
                ProgressUtils.Report(progressId, progress, "Waiting for all to be cached...");
                await Task.Delay(10);
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> CacheBuildConfigAtIndex(BuildTask task, int sourceIndex)
        {
            string directoryPath = Utils.CacheFolder;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            BuildConfig buildConfig = task.BuildConfigs[sourceIndex];
            string fullPath = buildConfig.Source().SourceFilePath();

            string sourcePath = fullPath;
            bool isDirectory = Utils.IsPathADirectory(sourcePath);
            if (!isDirectory)
            {
                if (fullPath.EndsWith(".exe"))
                {
                    // Given a .exe. use the Folder because they likely want to upload the entire folder - not just the .exe
                    sourcePath = Path.GetDirectoryName(fullPath);
                }
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogWarning($"Source path is empty for build config index: {sourceIndex}");
                return false;
            }

            // Files export to       /BuildUploader/CachedBuilds/FileName_GUID/FileName.extension
            // Directories export to /BuildUploader/CachedBuilds/DirectoryName_GUID/...
            string cacheFolderName = isDirectory ? new DirectoryInfo(sourcePath).Name : Path.GetFileNameWithoutExtension(sourcePath);
            string cacheFolderPath = Path.Combine(directoryPath, cacheFolderName + "_" + buildConfig.GUID);
            if (Directory.Exists(cacheFolderPath))
            {
                Debug.LogWarning($"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
                Directory.Delete(cacheFolderPath, true);
            }
            Directory.CreateDirectory(cacheFolderPath);
            
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            if (isDirectory)
            {
                bool copiedSuccessfully = await Utils.CopyDirectoryAsync(sourcePath, cacheFolderPath);
                if (!copiedSuccessfully)
                {
                    return false;
                }
            }
            else if (sourcePath.EndsWith(".zip"))
            {
                try
                {
                    // Unzip to a different location
                    // BuildUploader/CachedBuilds/ZipFileName_GUID/...
                    ZipUtils.UnZip(fullPath, cacheFolderPath);
                }
                catch (IOException e)
                {
                    Debug.LogException(e);
                    return false;
                }
            }
            else
            {
                // Getting a file - put it in its own folder
                // BuildUploader/CachedBuilds/FileName_GUID/FileName.extension
                string copiedFilePath = Path.Combine(cacheFolderPath, Path.GetFileName(sourcePath));
                await Utils.CopyFileAsync(sourcePath, copiedFilePath);
            }
            
            task.CachedLocations[sourceIndex] = cacheFolderPath;

            return true;
        }

        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to Cache Sources! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}