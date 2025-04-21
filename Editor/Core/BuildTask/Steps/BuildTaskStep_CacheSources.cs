using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    public class BuildTaskStep_CacheSources : ABuildTask_Step
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

        private async Task<bool> CacheBuildConfigAtIndex(BuildTask task, int configIndex)
        {
            string directoryPath = Utils.CacheFolder;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            BuildConfig buildConfig = task.BuildConfigs[configIndex];
            
            // Files export to /BuildUploader/CachedBuilds/GUID/*.*
            string cacheFolderPath = Path.Combine(directoryPath, buildConfig.GUID);
            for (var i = 0; i < buildConfig.Sources.Count; i++)
            {
                var sourceData = buildConfig.Sources[i];
                if (!sourceData.Enabled)
                {
                    continue;
                }

                bool cached = await CacheSource(sourceData, configIndex, i, cacheFolderPath);
                if (!cached)
                {
                    return false;
                }
            }

            task.CachedLocations[configIndex] = cacheFolderPath;

            return true;
        }
        
        private async Task<bool> CacheSource(BuildConfig.SourceData sourceData, int configIndex, int sourceIndex, string cacheFolderPath)
        {
            string sourcePath = sourceData.Source.SourceFilePath();
            bool sourceIsADirectory = Utils.IsPathADirectory(sourcePath);
            if (!sourceIsADirectory)
            {
                if (sourcePath.EndsWith(".exe"))
                {
                    // Given a .exe. use the Folder because they likely want to upload the entire folder - not just the .exe
                    sourcePath = Path.GetDirectoryName(sourcePath);
                }
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogWarning($"Source path is empty for Build Config index: {configIndex} and Source index: {sourceIndex}");
                return false;
            }

            // BuildUploader/CachedBuilds/GUID/
            string outputDirectory = cacheFolderPath;
            if (Directory.Exists(cacheFolderPath))
            {
                Debug.LogWarning($"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
                Directory.Delete(cacheFolderPath, true);
            }
            Directory.CreateDirectory(cacheFolderPath);
            
            // BuildUploader/CachedBuilds/GUID/Last Message_Data\StreamingAssets
            if (string.IsNullOrEmpty(sourceData.SubFolderPath))
            {
                outputDirectory = Path.Combine(cacheFolderPath, sourceData.SubFolderPath);
                Directory.CreateDirectory(outputDirectory);
            }
            
            
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            if (sourceIsADirectory)
            {
                bool copiedSuccessfully = await Utils.CopyDirectoryAsync(sourcePath, outputDirectory);
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
                    // BuildUploader/CachedBuilds/GUID/...
                    ZipUtils.UnZip(sourcePath, outputDirectory);
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
                // BuildUploader/CachedBuilds/GUID/FileName.extension
                string copiedFilePath = Path.Combine(outputDirectory, Path.GetFileName(sourcePath));
                await Utils.CopyFileAsync(sourcePath, copiedFilePath);
            }

            return true;
        }

        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to Cache Sources! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}