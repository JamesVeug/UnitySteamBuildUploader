using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    /// <summary>
    /// Download a build from UnityCloud
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(UnityCloudSource), "sources", "Downloads a zipped build from Unity Cloud")]
    [UploadSource("UnityCloud", "Choose Unity Cloud Build...")]
    public partial class UnityCloudSource : AUploadSource
    {
        [Wiki("Target", "Which Build Target to select a build from off Unity Cloud. eg: Windows/Mac")]
        private UnityCloudTarget sourceTarget;
        
        [Wiki("Build", "Which Build to download.")]
        private UnityCloudBuild sourceBuild;

        private Vector2 buildScrollPosition;

        private string downloadedFilePath;
        private string sourceFilePath;

        public UnityCloudSource()
        {
            // Required for reflection
        }
        
        public UnityCloudSource(UnityCloudTarget target, UnityCloudBuild build)
        {
            sourceTarget = target;
            sourceBuild = build;
        }

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Preparing
            string buildName = sourceBuild.platform + "-" + sourceBuild.buildtargetid + "-" + sourceBuild.build;
            string directoryPath = Path.Combine(Preferences.CacheFolderPath, "UnityCloudBuilds");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            downloadedFilePath = Path.Combine(directoryPath, buildName + ".zip");

            // Only download if we don't have it
            if (!File.Exists(downloadedFilePath))
            {
                string downloadUrl = sourceBuild.GetGameArtifactDownloadUrl();
                if (downloadUrl == null)
                {
                    downloadUrl = sourceBuild.GetAddressableArtifactDownloadUrl();
                    if (downloadUrl == null)
                    {
                        stepResult.SetFailed("Could not download UnityCloudBuild. No artifacts in build!");
                        return false;
                    }
                }

                stepResult.AddLog("Downloading from: " + downloadUrl);

                UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
                UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();

                // Wait for it to be downloaded?
                while (!webRequest.isDone)
                {
                    stepResult.SetPercentComplete(webRequest.progress);
                    stepResult.AddLog("Downloading UnityCloud Build: " + webRequest.progress * 100 + "%");
                    await Task.Yield();
                }

                // Save


                try
                {
                    await IOUtils.WriteAllBytesAsync(downloadedFilePath, request.downloadHandler.data);
                }
                catch (Exception e)
                {
                    stepResult.SetFailed("Failed to save downloaded file: " + downloadedFilePath + "\n" + e.Message);
                    return false;
                }
            }
            else
            {
                stepResult.AddLog("Skipping downloading form UnityCloud since it already exists: " + downloadedFilePath);
            }

            stepResult.AddLog("Retrieved UnityCloud Build: " + downloadedFilePath);

            // Record where the game is saved to
            sourceFilePath = downloadedFilePath;
            return true;
        }

        public override Task CleanUp(int i, UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            base.CleanUp(i, result, ctx);
            if (File.Exists(downloadedFilePath))
            {
                try
                {
                    result.AddLog("Deleting cached file: " + downloadedFilePath);
                    File.Delete(downloadedFilePath);
                }
                catch (IOException e)
                {
                    result.AddError("Failed to delete file: " + downloadedFilePath + "\n" + e.Message);
                }
            }
            
            return Task.CompletedTask;
        }

        public override string SourceFilePath()
        {
            return sourceFilePath;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (!InternalUtils.GetService<UnityCloudService>().IsReadyToStartBuild(out string reason))
            {
                errors.Add(reason);
            }
            
            if (sourceBuild == null)
            {
                errors.Add("No build selected");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["sourceTarget"] = sourceTarget?.name,
                ["sourceBuild"] = sourceBuild?.Id
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            string sourceTargetName = (string)data["sourceTarget"];

            List<UnityCloudTarget> buildTargets = UnityCloudAPI.CloudBuildTargets;
            if (buildTargets != null)
            {
                for (int i = 0; i < buildTargets.Count; i++)
                {
                    if (buildTargets[i].name == sourceTargetName)
                    {
                        sourceTarget = buildTargets[i];
                        List<UnityCloudBuild> builds = UnityCloudAPI.GetBuildsForTarget(sourceTarget);
                        if (builds != null)
                        {
                            for (int j = 0; j < builds.Count; j++)
                            {
                                if (builds[j].Id == (long)data["sourceBuild"])
                                {
                                    sourceBuild = builds[j];
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}