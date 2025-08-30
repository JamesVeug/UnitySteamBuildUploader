using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;

namespace Wireframe
{
    public class UploadTaskStep_CacheSources : AUploadTask_Step
    {
        public UploadTaskStep_CacheSources(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.CacheSources;
        
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<UploadConfig> buildConfigs = uploadTask.UploadConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = CacheBuildConfigAtIndex(uploadTask, j, report);
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
                await Task.Yield();
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> CacheBuildConfigAtIndex(UploadTask task, int configIndex, UploadTaskReport report)
        {
            UploadConfig uploadConfig = task.UploadConfigs[configIndex];
            UploadTaskReport.StepResult[] reports = report.NewReports(Type, uploadConfig.Sources.Count);

            AUploadModifer[] modifiers = uploadConfig.Modifiers
                .Where(a => a.Enabled && a.Modifier != null)
                .Select(a => a.Modifier)
                .ToArray();

            // Files export to /BuildUploader/CachedBuilds/ConfigGUID/*.*
            string sanitisedName = InternalEditorUtility.RemoveInvalidCharsFromFileName(task.UploadName, false);
            string cacheFolderPath = Path.Combine(Preferences.CacheFolderPath, "UploadTasks", $"{sanitisedName} ({task.GUID})", uploadConfig.GUID);
            bool pathAlreadyExists = Directory.Exists(cacheFolderPath);
            
            int sourceIndex = 0;
            task.CachedLocations[configIndex] = cacheFolderPath;
            for (var i = 0; i < uploadConfig.Sources.Count; i++)
            {
                var sourceData = uploadConfig.Sources[i];
                UploadTaskReport.StepResult result = reports[i];
                if (!sourceData.Enabled)
                {
                    result.AddLog("Skipping cacheSources because it's disabled");
                    continue;
                }

                // BuildUploader/CachedBuilds/GUID/
                if (sourceIndex++ == 0)
                {
                    if (pathAlreadyExists)
                    {
                        result.AddWarning(
                            $"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
                        Directory.Delete(cacheFolderPath, true);
                    }
                }

                string subCacheFolder = cacheFolderPath;
                if (!string.IsNullOrEmpty(sourceData.ExportFolder))
                {
                    subCacheFolder = Path.Combine(subCacheFolder, StringFormatter.FormatString(sourceData.ExportFolder, uploadConfig.Context));
                }
                
                if (!Directory.Exists(subCacheFolder))
                {
                    Directory.CreateDirectory(subCacheFolder);
                }

                try
                {
                    bool cached = await CacheSource(sourceData, modifiers, configIndex, i, subCacheFolder, result);
                    if (!cached)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    result.AddException(e);
                    result.SetFailed("Failed to cache source: " + sourceData.Source.SourceFilePath() + ".\n\n" + e.Message);
                    return false;
                }
                finally
                {
                    result.SetPercentComplete(1);
                }
            }

            return true;
        }
        
        private async Task<bool> CacheSource(UploadConfig.SourceData sourceData, AUploadModifer[] modifiers,
            int configIndex, int sourceIndex, string cacheFolderPath, UploadTaskReport.StepResult result)
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


            bool IgnorePath(string path)
            {
                // Check if any modifiers want to ignore this file
                foreach (AUploadModifer modifier in modifiers)
                {
                    if (modifier.IgnoreFileDuringCacheSource(path, configIndex, result))
                    {
                        result.AddLog($"Skipping copying source file {path} because it was ignored by modifier: {modifier.GetType().Name}");
                        return true;
                    }
                }

                return false;
            }
            
            
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            if (sourceIsADirectory)
            {
                result.AddLog($"Copying source directory {sourcePath} to {cacheFolderPath}");
                bool copiedSuccessfully = await Utils.CopyDirectoryAsync(sourcePath, cacheFolderPath, sourceData.DuplicateFileHandling, result, IgnorePath);
                return copiedSuccessfully;
            }

            if (IgnorePath(sourcePath))
            {
                return true;
            }
            
            if (sourcePath.EndsWith(".zip") && Preferences.AutoDecompressZippedSourceFiles)
            {
                // Unzip to a different location
                // BuildUploader/CachedBuilds/GUID/...
                result.AddLog("Auto-Decompressing .zip source file " + sourcePath + " to " + cacheFolderPath + ".\n" +
                              "To disable this feature, go to Preferences > Build Uploader > Auto Decompress Zipped Source Files");
                bool unzippedSuccessfully = await ZipUtils.UnZip(sourcePath, cacheFolderPath, result);
                return unzippedSuccessfully;
            }
            else
            {
                // Getting a file - put it in its own folder
                // BuildUploader/CachedBuilds/GUID/FileName.extension
                string copiedFilePath = Path.Combine(cacheFolderPath, Path.GetFileName(sourcePath));
                result.AddLog($"Copying source file {sourcePath} to {copiedFilePath}");
                bool copiedFileSuccessfully = await Utils.CopyFileAsync(sourcePath, copiedFilePath, sourceData.DuplicateFileHandling, result);
                return copiedFileSuccessfully;
            }
        }
        
        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            ReportCachedFiles(uploadTask, report);

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