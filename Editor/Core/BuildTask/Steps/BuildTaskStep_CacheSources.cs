using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    public class BuildTaskStep_CacheSources : ABuildTask_Step
    {
        public override StepType Type => StepType.CacheSources;
        
        
        public override async Task<bool> Run(BuildTask buildTask, BuildTaskReport report)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = CacheBuildConfigAtIndex(buildTask, j, report);
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

        private async Task<bool> CacheBuildConfigAtIndex(BuildTask task, int configIndex, BuildTaskReport report)
        {
            string directoryPath = Utils.CacheFolder;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            BuildConfig buildConfig = task.BuildConfigs[configIndex];
            BuildTaskReport.StepResult[] reports = report.NewReports(Type, buildConfig.Sources.Count);

            // Files export to /BuildUploader/CachedBuilds/GUID/*.*
            string cacheFolderPath = Path.Combine(directoryPath, buildConfig.GUID);
            for (var i = 0; i < buildConfig.Sources.Count; i++)
            {
                var sourceData = buildConfig.Sources[i];
                BuildTaskReport.StepResult result = reports[i];
                if (!sourceData.Enabled)
                {
                    result.AddLog("Skipping cacheSources because it's disabled");
                    continue;
                }

                bool cached = await CacheSource(sourceData, configIndex, i, cacheFolderPath, result);
                if (!cached)
                {
                    return false;
                }
            }

            task.CachedLocations[configIndex] = cacheFolderPath;

            return true;
        }
        
        private async Task<bool> CacheSource(BuildConfig.SourceData sourceData, int configIndex, int sourceIndex,
            string cacheFolderPath, BuildTaskReport.StepResult result)
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
                result.AddWarning($"Source path is empty for Build Config index: {configIndex} and Source index: {sourceIndex}");
                result.SetFailed($"Source path is empty");
                return false;
            }

            // BuildUploader/CachedBuilds/GUID/
            string outputDirectory = cacheFolderPath;
            if (Directory.Exists(cacheFolderPath))
            {
                result.AddWarning($"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
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
                bool copiedSuccessfully = await Utils.CopyDirectoryAsync(sourcePath, outputDirectory, result);
                if (!copiedSuccessfully)
                {
                    result.AddError("Failed to copy directory: " + sourcePath + " to " + outputDirectory);
                    result.SetFailed("Failed to copy directory: " + sourcePath + " to " + outputDirectory);
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
                    result.AddException(e);
                    result.SetFailed("Failed to unzip file: " + sourcePath + " to " + outputDirectory);
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
        
        public override Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            ReportCachedFiles(buildTask, report);
            return Task.FromResult(true);
        }
    }
}