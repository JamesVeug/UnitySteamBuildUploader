using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// A Wrapper for a single upload
    /// Each task can contain multiple upload configs that will be executed in parallel
    /// When started a Report is created that contains all live information about the upload
    /// Listen to OnComplete to get the report when the upload is done.
    /// </summary>
    public partial class UploadTask
    {
        internal static List<UploadTask> AllTasks = new List<UploadTask>();
        
        public event Action<UploadTaskReport> OnComplete = delegate { };
        
        public string GUID => guid;
        public List<UploadConfig> UploadConfigs => uploadConfigs;
        public List<UploadConfig.UploadActionData> PreUploadActions => preUploadActions;
        public List<UploadConfig.UploadActionData> PostUploadActions => postUploadActions;
        public string UploadDescription => uploadDescription;
        public string UploadName => uploadName;
        public string[] CachedLocations => cachedLocations;
        public UploadTaskReport Report => report;
        
        public bool IsComplete { get; private set; }
        public bool IsSuccessful { get; private set; }
        public float PercentComplete { get; private set; }
        public AUploadTask_Step.StepType CurrentStep { get; private set; }
        public AUploadTask_Step[] CurrentSteps => m_CurrentSteps;
        
        public UploadTaskStringFormatterContext Context => context;

        private UploadTaskReport report;
        private List<UploadConfig> uploadConfigs;
        private List<UploadConfig.UploadActionData> preUploadActions;
        private List<UploadConfig.UploadActionData> postUploadActions;
        private UploadTaskStringFormatterContext context;
        private string[] cachedLocations;
        private int progressId;
        private int totalSteps;
        
        private string uploadName;
        private string uploadDescription;
        private string guid;
        private AUploadTask_Step[] m_CurrentSteps;

        public UploadTask(UploadProfile uploadProfile) : this(uploadProfile.ProfileName, uploadProfile.UploadConfigs)
        {
            
        }
        
        public UploadTask(string name, UploadConfig config) : this()
        {
            uploadName = name;

            AddConfig(config);
        }
        
        public UploadTask(string name, List<UploadConfig> uploadConfigs, List<UploadConfig.UploadActionData> preUploadActions = null, List<UploadConfig.UploadActionData> postUploadActions = null) : this()
        {
            uploadName = name;

            if (preUploadActions != null)
            {
                foreach (UploadConfig.UploadActionData action in preUploadActions)
                {
                    AddPreUploadAction(action);
                }
            }

            if (postUploadActions != null)
            {
                foreach (UploadConfig.UploadActionData action in postUploadActions)
                {
                    AddPostUploadAction(action);
                }
            }

            foreach (UploadConfig config in uploadConfigs)
            {
                AddConfig(config);
            }
        }

        public UploadTask(List<UploadConfig> uploadConfigs, List<UploadConfig.UploadActionData> preUploadActions = null, List<UploadConfig.UploadActionData> postUploadActions = null)
            : this("No Name Specified", uploadConfigs, preUploadActions, postUploadActions)
        {
            
        }
        
        public UploadTask()
        {
            guid = Guid.NewGuid().ToString().Substring(0, 6);
            uploadDescription = "";
            uploadConfigs = new List<UploadConfig>();
            preUploadActions = new List<UploadConfig.UploadActionData>();
            postUploadActions = new List<UploadConfig.UploadActionData>();
            
            context = new UploadTaskStringFormatterContext(this);
            context.AddCommand(Wireframe.Context.TASK_PROFILE_NAME_KEY, () => uploadName);
            context.AddCommand(Wireframe.Context.TASK_DESCRIPTION_KEY, () => uploadDescription);
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
            report = new UploadTaskReport(guid, UploadName, invokeDebugLogs);
            cachedLocations = new string[uploadConfigs.Count];
            IsComplete = false;
            PercentComplete = 0f;
            IsSuccessful = false;
            CurrentStep = AUploadTask_Step.StepType.Validation;
            BuildUploaderProjectSettings.BumpUploadNumber();

            context.CacheCallbacks();
            for (var i = 0; i < uploadConfigs.Count; i++)
            {
                uploadConfigs[i].Context.SetParent(context);
                uploadConfigs[i].Context.CacheCallbacks();
            }

            AUploadTask_Step[] steps = new AUploadTask_Step[]
            {
                // Executed in order and stops when 1 fails
                new UploadTaskStep_PrepareSources(context), // Make sure the sources are ready before we retrieve them
                new UploadTaskStep_GetSources(context), // Download content from services or get local folder
                new UploadTaskStep_CacheSources(context), // Cache the content in Utils.CachePath
                new UploadTaskStep_ModifyCachedSources(context), // Modify the build so it's ready to be uploaded (Remove/add files)
                new UploadTaskStep_PrepareDestinations(context), // Make sure the destination is ready to receive the content
                new UploadTaskStep_Upload(context), // Upload cached content,
                
                // Always executed
                new UploadTaskStep_PostUploadActions(context), // Do any post upload actions that are not related to the build
                new UploadTaskStep_Cleanup(context), // Cleanup the task, delete cached files, etc.
            };
            m_CurrentSteps = steps;
            totalSteps = steps.Length;
            
            // Setup Context to display task specific strings
            SetupContext(report);
            
            // Run the Build Uploader
            await Execute(steps);

            IsComplete = true;
            PercentComplete = 1f;
            IsSuccessful = report.Successful;
            
            ProgressUtils.Remove(progressId);
            report.Complete();
            OnComplete?.Invoke(report);
        }

        private async Task Execute(AUploadTask_Step[] steps)
        {
            // Validate
            bool valid = false;
            try{
                valid = Validate();
            } 
            catch (Exception e)
            {
                UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.Validation);
                result.AddException(e);
                valid = false;
            }
            
            if (!valid)
            {
                UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.Validation);
                result.SetFailed("Validation failed. See errors above.");
                SetProgress(0, 1f, CurrentStep.ToString());
                return;
            }
            
            // Do upload steps
            bool allStepsSuccessful = true;
            CancellationTokenSource token = new CancellationTokenSource();
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
                Task<bool> intraTask = step.Run(this, report, token);
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
                if (!stepSuccessful || !postStepSuccessful || token.IsCancellationRequested)
                {
                    allStepsSuccessful = false;
                }
            }
        }

        private bool Validate()
        {
            report.SetProcess(AUploadTask_Step.StepProcess.Intra);

            bool valid = true;
            UploadTaskReport.StepResult[] reports = report.NewReports(AUploadTask_Step.StepType.Validation, preUploadActions.Count);
            for (var i = 0; i < preUploadActions.Count; i++)
            {
                var action = preUploadActions[i];
                if (action.WhenToExecute == UploadConfig.UploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }

                UploadTaskReport.StepResult result = reports[i];
                if (action.UploadAction == null)
                {
                    result.AddError($"No pre upload action specified at index {i}");
                    valid = false;
                    continue;
                }

                List<string> errors = new List<string>();
                action.UploadAction.TryGetErrors(errors);
                foreach (string error in errors)
                {
                    result.AddError(error);
                    valid = false;
                }
            }
            
            reports = report.NewReports(AUploadTask_Step.StepType.Validation, uploadConfigs.Count);
            for (var i = 0; i < uploadConfigs.Count; i++)
            {
                UploadTaskReport.StepResult result = reports[i];
                UploadConfig config = uploadConfigs[i];
                if (!config.Enabled)
                {
                    continue;
                }
                
                List<string> errors = config.GetAllErrors();
                if (errors.Count > 0)
                {
                    foreach (string error in errors)
                    {
                        result.AddError(error);
                        valid = false;
                    }
                }
                else
                {
                    result.AddLog("No errors found in config: " + config.GUID);
                }
                
                List<string> warnings = config.GetAllWarnings();
                if (warnings.Count > 0)
                {
                    foreach (string warning in warnings)
                    {
                        result.AddWarning(warning);
                    }
                }
            }
            
            reports = report.NewReports(AUploadTask_Step.StepType.Validation, postUploadActions.Count);
            for (var i = 0; i < postUploadActions.Count; i++)
            {
                var action = postUploadActions[i];
                if (action.WhenToExecute == UploadConfig.UploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }

                UploadTaskReport.StepResult result = reports[i];
                if (action.UploadAction == null)
                {
                    result.AddError($"No post upload action specified at index {i}");
                    valid = false;
                    continue;
                }

                List<string> errors = new List<string>();
                action.UploadAction.TryGetErrors(errors);
                foreach (string error in errors)
                {
                    result.AddError(error);
                    valid = false;
                }
            }

            return valid;
        }

        private void SetProgress(int step, float percent, string message)
        {
            float mainPercent = (float)step / totalSteps;
            float subPercent = (1f / totalSteps) * Mathf.Clamp01(percent);
            
            PercentComplete = mainPercent + subPercent;
            ProgressUtils.Report(progressId, percent, message);
        }

        private void SetupContext(UploadTaskReport report)
        {
            context.AddCommand(Wireframe.Context.TASK_FAILED_REASONS_KEY, ()=>
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
            });
        }

        public void AddConfig(UploadConfig config)
        {
            if (config == null)
            {
                return;
            }

            config.Context.SetParent(context);
            
            uploadConfigs.Add(config);
        }
        
        public void AddPreUploadAction(UploadConfig.UploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            preUploadActions.Add(action);
        }

        public void AddPreUploadAction(AUploadAction action, UploadConfig.UploadActionData.UploadCompleteStatus whenToExecute = UploadConfig.UploadActionData.UploadCompleteStatus.Always)
        {
            if (action == null)
            {
                return;
            }
            
            preUploadActions.Add(new UploadConfig.UploadActionData(action, whenToExecute));
        }
        
        public void AddPostUploadAction(UploadConfig.UploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            postUploadActions.Add(action);
        }
        
        public void AddPostUploadAction(AUploadAction action, UploadConfig.UploadActionData.UploadCompleteStatus whenToExecute = UploadConfig.UploadActionData.UploadCompleteStatus.Always)
        {
            if (action == null)
            {
                return;
            }

            postUploadActions.Add(new UploadConfig.UploadActionData(action, whenToExecute));
        }
        
        public void SetBuildDescription(string description)
        {
            uploadDescription = description;
        }
        
        internal void SetReport(UploadTaskReport report)
        {
            this.report = report;
            guid = report.GUID;
            uploadName = report.Name;
            IsSuccessful = report.Successful;
            IsComplete = true;
            PercentComplete = 1f;
        }
    }
}