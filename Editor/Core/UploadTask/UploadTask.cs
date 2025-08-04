using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    public class UploadTask
    {
        internal static List<UploadTask> AllTasks = new List<UploadTask>();
        
        public event Action<UploadTaskReport> OnComplete = delegate { };
        
        public string GUID => guid;
        public List<UploadConfig> UploadConfigs => uploadConfigs;
        public List<UploadConfig.PostUploadActionData> PostUploadActions => postUploadActions;
        public string BuildDescription => buildDescription;
        public string[] CachedLocations => cachedLocations;
        public UploadTaskReport Report => report;
        
        public bool IsComplete { get; private set; }
        public bool IsSuccessful { get; private set; }
        public float PercentComplete { get; private set; }
        public AUploadTask_Step.StepType CurrentStep { get; private set; }
        public StringFormatter.Context Context => context;

        private UploadTaskReport report;
        private List<UploadConfig> uploadConfigs;
        private List<UploadConfig.PostUploadActionData> postUploadActions;
        private StringFormatter.Context context;
        private string[] cachedLocations;
        private int progressId;
        private int totalSteps;
        
        private string buildDescription;
        private string guid;

        public UploadTask(UploadProfile uploadProfile, string buildDescription) : this()
        {
            this.buildDescription = buildDescription;
            this.uploadConfigs = uploadProfile.UploadConfigs ?? new List<UploadConfig>();
            this.postUploadActions = uploadProfile.PostUploadActions ?? new List<UploadConfig.PostUploadActionData>();
        }

        public UploadTask(List<UploadConfig> uploadConfigs, string buildDescription, List<UploadConfig.PostUploadActionData> postUploadActions = null) : this()
        {
            this.buildDescription = buildDescription;
            this.uploadConfigs = uploadConfigs;
            this.postUploadActions = postUploadActions ?? new List<UploadConfig.PostUploadActionData>();
        }
        
        public UploadTask()
        {
            guid = Guid.NewGuid().ToString().Substring(0, 6);
            buildDescription = "";
            uploadConfigs = new List<UploadConfig>();
            postUploadActions = new List<UploadConfig.PostUploadActionData>();
            
            context = new StringFormatter.Context();
            context.TaskDescription = ()=>buildDescription;
            
            AllTasks.Add(this);
        }

        ~UploadTask()
        {
            if (ProgressUtils.Exists(progressId))
            {
                ProgressUtils.Remove(progressId);
            }
        }

        /// <summary>
        /// Start the upload task synchronously.
        /// Listen to OnComplete to get the report when the upload is done.
        /// </summary>
        /// <param name="invokeDebugLogs">When a log,warning,error occurs during the upload should this log to Unity Console? logs can be found in the report at the end.</param>
        public void Start(bool invokeDebugLogs = true)
        {
            _ = StartAsync(invokeDebugLogs);
        }
        
        /// <summary>
        /// Start the upload task synchronously.
        /// Listen to OnComplete to get the report when the upload is done. 
        /// </summary>
        /// <param name="invokeDebugLogs">When a log,warning,error occurs during the upload should this log to Unity Console? logs can be found in the report at the end.</param>
        /// <returns>Report regarding if the upload was successful or not aswell as all the logs</returns>
        public async Task StartAsync(bool invokeDebugLogs=true)
        {
            progressId = ProgressUtils.Start("Build Uploader Window", "Setting up...");
            report = new UploadTaskReport(guid, invokeDebugLogs);
            cachedLocations = new string[uploadConfigs.Count];
            IsComplete = false;
            PercentComplete = 0f;
            IsSuccessful = false;
            CurrentStep = AUploadTask_Step.StepType.GetSources;

            AUploadTask_Step[] steps = new AUploadTask_Step[]
            {
                // Executed in order and stops when 1 fails
                new UploadTaskStep_GetSources(context), // Download content from services or get local folder
                new UploadTaskStep_CacheSources(context), // Cache the content in Utils.CachePath
                new UploadTaskStep_ModifyCachedSources(context), // Modify the build so it's ready to be uploaded (Remove/add files)
                new UploadTaskStep_PrepareDestinations(context), // Make sure the destination is ready to receive the content
                new UploadTaskStep_Upload(context), // Upload cached content,
                
                // Always executed
                new UploadTaskStep_PostUploadActions(context), // Do any post upload actions that are not related to the build
                new UploadTaskStep_Cleanup(context), // Cleanup the task, delete cached files, etc.
            };
            totalSteps = steps.Length;
            
            // Setup Context to display task specific strings
            SetupContext(report);
            
            // Do upload steps
            bool allStepsSuccessful = true;
            for (int i = 0; i < steps.Length; i++)
            {
                AUploadTask_Step step = steps[i];
                CurrentStep = step.Type;
                if (!allStepsSuccessful && step.RequiresEverythingBeforeToSucceed)
                {
                    continue;
                }
                
                // Perform the Step (GetSources, CacheSources, etc.)
                report.SetProcess(AUploadTask_Step.StepProcess.Intra);
                Task<bool> intraTask = step.Run(this, report);
                while (!intraTask.IsCompleted)
                {
                    float progress = report.GetProgress(step.Type, AUploadTask_Step.StepProcess.Intra);
                    SetProgress(i, progress, CurrentStep.ToString());
                    await Task.Yield();
                }
                bool stepSuccessful = intraTask.Result;
                SetProgress(i, 1f, CurrentStep.ToString());
                
                // Post-step logic mainly for logging
                report.SetProcess(AUploadTask_Step.StepProcess.Post);
                bool postStepSuccessful = await step.PostRunResult(this, report);
                if (!stepSuccessful || !postStepSuccessful)
                {
                    allStepsSuccessful = false;
                }
            }

            IsComplete = true;
            PercentComplete = 1f;
            IsSuccessful = report.Successful;
            
            ProgressUtils.Remove(progressId);
            report.Complete();
            OnComplete?.Invoke(report);
        }
        
        private void SetProgress(int step, float percent, string message)
        {
            float mainPercent = (float)step / totalSteps;
            float subPercent = (1f / totalSteps) * Mathf.Clamp01(percent);
            
            PercentComplete = mainPercent + subPercent;
            // Debug.LogFormat("UploadTask Progress: {0:F2} {1:F2} = {2:F2}", step, percent, PercentComplete);
            ProgressUtils.Report(progressId, percent, message);
        }

        private void SetupContext(UploadTaskReport report)
        {
            context.UploadTaskFailText = () =>
            {
                if (report.Successful)
                {
                    return "Upload task did not fail.";
                }
                
                var failReasons = report.GetFailReasons();
                if (!failReasons.Any())
                {
                    return "No specific failure reasons provided.";
                }

                string reasonText = "";
                foreach ((AUploadTask_Step.StepType Key, string FailReason) reason in failReasons)
                {
                    if (string.IsNullOrEmpty(reasonText))
                    {
                        reasonText += $"{reason.Key}: {reason.FailReason}";
                    }
                    else
                    {
                        reasonText += $"\n{reason.Key}: {reason.FailReason}";
                    }
                }
                return reasonText;
            };
        }

        public void AddConfig(UploadConfig config)
        {
            if (config == null)
            {
                return;
            }
            
            uploadConfigs.Add(config);
        }
        
        public void AddPostUploadAction(UploadConfig.PostUploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            postUploadActions.Add(action);
        }
        
        public void SetBuildDescription(string description)
        {
            buildDescription = description;
        }
    }
}