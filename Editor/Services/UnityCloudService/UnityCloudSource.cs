using System.Collections.Generic;
using System.IO;
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
    [BuildSource("UnityCloud", "Choose Unity Cloud Build...")]
    public partial class UnityCloudSource : ABuildSource
    {
        [Wiki("Target", "Which Build Target to select a build from off Unity Cloud.")]
        private UnityCloudTarget sourceTarget;
        
        [Wiki("Build", "Which Build to download.")]
        private UnityCloudBuild sourceBuild;

        private Vector2 buildScrollPosition;

        private string downloadedFilePath;
        private string sourceFilePath;

        public override async Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult)
        {
            m_getSourceInProgress = true;
            m_downloadProgress = 0.0f;

            // Preparing
            m_progressDescription = "Preparing...";
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

                m_progressDescription = "Fetching...";
                UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
                UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();

                // Wait for it to be downloaded?
                while (!webRequest.isDone)
                {
                    await Task.Delay(10);
                    m_downloadProgress = request.downloadProgress;
                    m_progressDescription = "Downloading from UnityCloud...";
                }

                // Save
                m_progressDescription = "Saving locally...";
                
#if UNITY_2021_2_OR_NEWER
                await File.WriteAllBytesAsync(downloadedFilePath, request.downloadHandler.data);
#else
                File.WriteAllBytes(fullFilePath, request.downloadHandler.data);
#endif
            }
            else
            {
                stepResult.AddLog("Skipping downloading form UnityCloud since it already exists: " + downloadedFilePath);
            }

            m_progressDescription = "Done!";
            stepResult.AddLog("Retrieved UnityCloud Build: " + downloadedFilePath);

            // Record where the game is saved to
            sourceFilePath = downloadedFilePath;
            m_downloadProgress = 1.0f;
            return true;
        }

        public override void CleanUp(BuildTaskReport.StepResult result)
        {
            base.CleanUp(result);
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
        }

        public override string SourceFilePath()
        {
            return sourceFilePath;
        }

        public override float DownloadProgress()
        {
            return m_downloadProgress;
        }

        public override string ProgressTitle()
        {
            return "Downloading from UnityCloud";
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup(out string reason)
        {
            if (!InternalUtils.GetService<UnityCloudService>().IsReadyToStartBuild(out reason))
            {
                return false;
            }
            
            if (sourceBuild == null)
            {
                reason = "No build selected";
                return false;
            }

            reason = "";
            return true;
        }

        public override string GetBuildDescription()
        {
            return sourceBuild.CreateBuildName();
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["sourceTarget"] = sourceTarget?.name
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
                    }
                }
            }
        }
    }
}