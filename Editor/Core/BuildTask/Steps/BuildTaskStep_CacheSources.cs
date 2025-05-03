using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

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
            string directoryPath = Preferences.CacheFolderPath;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            BuildConfig buildConfig = task.BuildConfigs[configIndex];
            BuildTaskReport.StepResult[] reports = report.NewReports(Type, buildConfig.Sources.Count);

            // Files export to /BuildUploader/CachedBuilds/GUID/*.*
            int sourceIndex = 0;
            string cacheFolderPath = Path.Combine(directoryPath, buildConfig.GUID);
            task.CachedLocations[configIndex] = cacheFolderPath;
            for (var i = 0; i < buildConfig.Sources.Count; i++)
            {
                var sourceData = buildConfig.Sources[i];
                BuildTaskReport.StepResult result = reports[i];
                if (!sourceData.Enabled)
                {
                    result.AddLog("Skipping cacheSources because it's disabled");
                    continue;
                }

                // BuildUploader/CachedBuilds/GUID/
                if (sourceIndex++ == 0)
                {
                    if (Directory.Exists(cacheFolderPath))
                    {
                        result.AddWarning(
                            $"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
                        Directory.Delete(cacheFolderPath, true);
                    }
                }

                string sourcePath = cacheFolderPath;
                if (!string.IsNullOrEmpty(sourceData.ExportFolder))
                {
                    sourcePath = Path.Combine(sourcePath, sourceData.ExportFolder);
                }
                
                if (!Directory.Exists(sourcePath))
                {
                    Directory.CreateDirectory(sourcePath);
                }

                try
                {
                    bool cached = await CacheSource(sourceData, configIndex, i, sourcePath, result);
                    if (!cached)
                    {
                        return false;
                    }
                }
                catch (IOException e)
                {
                    result.AddException(e);
                    result.SetFailed("Failed to cache source: " + sourceData.Source.SourceFilePath());
                    return false;
                }
            }

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
            
            
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            if (sourceIsADirectory)
            {
                bool copiedSuccessfully = await Utils.CopyDirectoryAsync(sourcePath, cacheFolderPath, sourceData.DuplicateFileHandling, result);
                if (!copiedSuccessfully)
                {
                    return false;
                }
            }
            else if (sourcePath.EndsWith(".zip") && Preferences.AutoDecompressZippedSourceFiles)
            {
                // Unzip to a different location
                // BuildUploader/CachedBuilds/GUID/...
                result.AddLog("Auto-Decompressing .zip source file " + sourcePath + " to " + cacheFolderPath + ".\n" +
                              "To disable this feature, go to Preferences > Build Uploader > Auto Decompress Zipped Source Files");
                return await ZipUtils.UnZip(sourcePath, cacheFolderPath, result);
            }
            else
            {
                // Getting a file - put it in its own folder
                // BuildUploader/CachedBuilds/GUID/FileName.extension
                string copiedFilePath = Path.Combine(cacheFolderPath, Path.GetFileName(sourcePath));
                return await Utils.CopyFileAsync(sourcePath, copiedFilePath, sourceData.DuplicateFileHandling, result);
            }

            return true;
        }
        
        public override Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            ReportCachedFiles(buildTask, report);

            if (!report.Successful)
            {
                foreach (var failReason in report.GetFailReasons())
                {
                    if (failReason.Key == StepType.CacheSources && failReason.FailReason.Contains("Failed to copy directory"))
                    {
                        EditorUtility.DisplayDialog("Build Uploader", "Failed to copy directory.\n\n" +
                            "This is likely because the path is too long.\n\n" +
                            "Try changing the Cache directory in Preferences to a shorter path and try again.", "OK");
                        break;
                    }
                }
            }
            return Task.FromResult(true);
        }
    }
}