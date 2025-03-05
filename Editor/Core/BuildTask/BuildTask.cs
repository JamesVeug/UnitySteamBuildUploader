using System;
using System.Collections.Generic;
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
            progressId = ProgressUtils.Start("Build Uploader Window", "Starting up...");

            ABuildTask_Step[] steps = new ABuildTask_Step[]
            {
                new BuildTaskStep_GetSources(), // Download content from services or get local folder
                new BuildTaskStep_CacheSources(), // Cache the content in Utils.CachePath
                new BuildTaskStep_PrepareDestinations(), // Make sure the destination is ready to receive the content
                new BuildTaskStep_Upload() // Upload cached content
            };
            
            bool successful = true;
            for (int i = 0; i < steps.Length; i++)
            {
                ProgressUtils.Report(progressId, (float)i/steps.Length, steps[i].Name);
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
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                ProgressUtils.Report(progressId, 1f, "Cleaning up...");
                for (int j = 0; j < buildConfigs.Count; j++)
                {
                    if (buildConfigs[j].Enabled)
                    {
                        buildConfigs[j].Source().CleanUp();
                        buildConfigs[j].Destination().CleanUp();
                    }
                }
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