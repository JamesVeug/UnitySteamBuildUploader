using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildTask
    {
        public List<BuildConfig> BuildConfigs => buildConfigs;
        public string BuildDescription => buildDescription;
        public string[] CachedLocations => cachedLocations;
        
        private List<BuildConfig> buildConfigs;
        private string[] cachedLocations;
        private int progressId;
        private string buildDescription;

        public BuildTask(List<BuildConfig> buildConfigs, string buildDescription)
        {
            this.buildDescription = buildDescription;
            this.buildConfigs = buildConfigs;
            this.cachedLocations = new string[buildConfigs.Count];
        }

        ~BuildTask()
        {
            if (ProgressUtils.Exists(progressId))
            {
                ProgressUtils.Remove(progressId);
            }
        }

        public async Task Start(Action tick = null)
        {
            progressId = ProgressUtils.Start("Build Uploader Window", "Upload Builds");

            ABuildTask_Step[] steps = new ABuildTask_Step[]
            {
                new BuildTaskStep_GetSources(), // Download content from services or get local folder
                new BuildTaskStep_CacheSources(), // Cache the content in Utils.CachePath
                new BuildTaskStep_ModifyCachedSources(), // Modify the build so it's ready to be uploaded (Remove/add files)
                new BuildTaskStep_PrepareDestinations(), // Make sure the destination is ready to receive the content
                new BuildTaskStep_Upload() // Upload cached content
            };
            
            bool successful = true;
            for (int i = 0; i < steps.Length; i++)
            {
                ProgressUtils.Report(progressId, (float)i/(steps.Length+1), "Upload Builds");
                Task<bool> task = steps[i].Run(this);
                while (!task.IsCompleted)
                {
                    tick?.Invoke();
                    await Task.Delay(10);
                }

                if (!task.Result)
                {
                    steps[i].Failed(this);
                    // DisplayDialog("Failed to " + steps[i].Name + "! Not uploading any builds.\n\nSee logs for more info.", "Okay");
                    successful = false;
                    break;
                }
            }
            
            if (successful)
            {
                DisplayDialog("All builds uploaded successfully!", "Yay!");
            }
            
            // Cleanup to make sure nothing is left behind - dirtying up the users computer
            ProgressUtils.Report(progressId, (float)steps.Length/(steps.Length+1), "Cleaning up");
            if (Preferences.DeleteCacheAfterBuild)
            {
                // Delete cache
                int cleanupProgressId = ProgressUtils.Start("Cleanup", "Deleting cached files");
                for (var i = 0; i < cachedLocations.Length; i++)
                {
                    var cachedLocation = cachedLocations[i];
                    if (!Directory.Exists(cachedLocation))
                    {
                        continue;
                    }
                    
                    await Task.Yield();
                    ProgressUtils.Report(cleanupProgressId, 0, $"Deleting cached files " + (i+1) + "/" + cachedLocations.Length);
                    
                    Directory.Delete(cachedLocation, true);
                }

                // Cleanup configs
                ProgressUtils.Report(cleanupProgressId, 0.5f, "Cleaning up configs");
                for (int i = 0; i < buildConfigs.Count; i++)
                {
                    await Task.Yield();
                    ProgressUtils.Report(cleanupProgressId, 0.5f, $"Cleaning up configs " + (i+1) + "/" + buildConfigs.Count);
                    
                    if (buildConfigs[i].Enabled)
                    {
                        buildConfigs[i].CleanUp();
                    }
                }
                
                ProgressUtils.Remove(cleanupProgressId);
            }
            else
            {
                Debug.Log("Skipping cache and build cleanup. Re-enable in preferences.");
            }
                
            ProgressUtils.Remove(progressId);
        }

        public void DisplayDialog(string message, string buttonText)
        {
            Debug.Log(message);
            EditorUtility.DisplayDialog("Build Uploader", message, buttonText);
        }
    }
}