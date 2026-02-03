using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// A Wrapper for a single upload
    /// Each task can contain multiple upload configs that will be executed in parallel
    /// When started, a Report is created that contains all live information about the upload
    /// Listen to OnComplete to get the report when the upload is done.
    /// </summary>
    public partial class UploadTask
    {
        internal static List<UploadTask> AllTasks = new List<UploadTask>();
        
        public event Action<UploadTaskReport> OnComplete = delegate { };
        
        public string GUID => m_guid;
        public List<UploadConfig> UploadConfigs => m_uploadConfigs;
        public List<UploadConfig.UploadActionData> Actions => m_actions;
        public string UploadDescription => m_uploadDescription;
        public string UploadName => m_uploadName;
        public string[] CachedLocations => m_cachedLocations;
        public UploadTaskReport Report => m_report;
        
        public bool IsComplete { get; private set; }
        public bool HasStarted { get; private set; }
        public bool IsSuccessful { get; private set; }
        public float PercentComplete { get; private set; }
        public AUploadTask_Step.StepType CurrentStepType { get; private set; }
        public AUploadTask_Step CurrentStep { get; private set; }
        public AUploadTask_Step[] CurrentSteps => m_currentSteps;
        
        public UploadTaskStringFormatterContext Context => m_context;

        private UploadTaskReport m_report;
        private List<UploadConfig> m_uploadConfigs;
        private List<UploadConfig.UploadActionData> m_actions;
        private UploadTaskStringFormatterContext m_context;
        private string[] m_cachedLocations;
        private int m_progressId;
        private int m_totalSteps;
        
        private string m_uploadName;
        private string m_uploadDescription;
        private string m_guid;
        private AUploadTask_Step[] m_currentSteps;

        public UploadTask(UploadProfile uploadProfile) : 
            this(uploadProfile.ProfileName, uploadProfile.UploadConfigs, uploadProfile.Actions)
        {
            
        }
        
        public UploadTask(string name, UploadConfig config) : this()
        {
            m_uploadName = name;

            AddConfig(config);
        }
        
        public UploadTask(string name, List<UploadConfig> uploadConfigs, List<UploadConfig.UploadActionData> actions = null) 
            : this()
        {
            m_uploadName = name;

            AddActions(actions);

            foreach (UploadConfig config in uploadConfigs)
            {
                AddConfig(config);
            }
        }

        public UploadTask(List<UploadConfig> uploadConfigs, List<UploadConfig.UploadActionData> actions = null)
            : this("No Name Specified", uploadConfigs, actions)
        {
            
        }
        
        public UploadTask()
        {
            m_guid = Guid.NewGuid().ToString().Substring(0, 6);
            m_uploadDescription = "";
            m_uploadConfigs = new List<UploadConfig>();
            m_actions = new List<UploadConfig.UploadActionData>();
            
            m_context = new UploadTaskStringFormatterContext(this);
        }

        ~UploadTask()
        {
            if (ProgressUtils.Exists(m_progressId))
            {
                ProgressUtils.Remove(m_progressId);
            }
        }

        /// <summary>
        /// Start the upload task asynchronously.
        /// Listen to OnComplete to get the report when the upload is done.
        /// </summary>
        /// <param name="invokeDebugLogs">When a log,warning,error occurs during the upload should this log to Unity Console? logs can be found in the report at the end.</param>
        public void Start(bool invokeDebugLogs = true)
        {
            _ = StartAsync(invokeDebugLogs);
        }

        /// <summary>
        /// Start the upload task synchronously.
        /// </summary>
        /// <param name="invokeDebugLogs">When a log,warning,error occurs during the upload should this log to Unity Console? logs can be found in the report at the end.</param>
        public void StartBlock(bool invokeDebugLogs = true)
        {
            StartAsync(invokeDebugLogs).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Start the upload task asynchronously.
        /// Listen to OnComplete to get the report when the upload is done. 
        /// </summary>
        /// <param name="invokeDebugLogs">When a log,warning,error occurs during the upload should this log to Unity Console? logs can be found in the report at the end.</param>
        /// <returns>Report regarding if the upload was successful or not aswell as all the logs</returns>
        public async Task StartAsync(bool invokeDebugLogs=true)
        {
            m_progressId = ProgressUtils.Start("Build Uploader Window", "Setting up...");
            m_report = new UploadTaskReport(m_guid, UploadName, invokeDebugLogs);
            m_cachedLocations = new string[m_uploadConfigs.Count];
            IsComplete = false;
            PercentComplete = 0f;
            IsSuccessful = false;
            HasStarted = true;
            CurrentStepType = AUploadTask_Step.StepType.Validation;
            BuildUploaderProjectSettings.BumpUploadNumber();

            for (var i = 0; i < m_uploadConfigs.Count; i++)
            {
                m_uploadConfigs[i].SetContextAndCacheCallbacks(m_context);
            }
            m_context.CacheCallbacks();
            
            for (var i = 0; i < m_actions.Count; i++)
            {
                if (m_actions[i].UploadAction != null)
                {
                    m_actions[i].UploadAction.Context.SetParent(m_context);
                    m_actions[i].UploadAction.Context.CacheCallbacks();
                }
            }

            // Executed in order
            AUploadTask_Step[] steps = new AUploadTask_Step[]
            {
                // Setup - Always executed
                new UploadTaskStep_Validate(m_context), // Make sure the sources are ready before we retrieve them
                new UploadTaskStep_PreUploadActions(m_context), // Execute actions before we begin the upload progress
                
                // Do the upload sequence - Only executed if all previous steps succeeded
                new UploadTaskStep_PrepareSources(m_context), // Make sure the sources are ready before we retrieve them
                new UploadTaskStep_GetSources(m_context), // Download content from services or get local folder
                new UploadTaskStep_CacheSources(m_context), // Cache the content in Utils.CachePath
                new UploadTaskStep_ModifyCachedSources(m_context), // Modify the build so it's ready to be uploaded (Remove/add files)
                new UploadTaskStep_PrepareDestinations(m_context), // Make sure the destination is ready to receive the content
                new UploadTaskStep_Upload(m_context), // Upload cached content,
                
                // Cleanup - Always executed
                new UploadTaskStep_PostUploadActions(m_context), // Execute actions after we finish the upload progress
                new UploadTaskStep_Cleanup(m_context), // Clean up the task, delete cached files, etc.
            };
            m_currentSteps = steps;
            m_totalSteps = steps.Length;
            
            // Set up Context to display task-specific strings
            SetupContext(m_report);
            
            // Run the Build Uploader
            await Execute(steps);

            IsComplete = true;
            PercentComplete = 1f;
            IsSuccessful = m_report.Successful;
            
            ProgressUtils.Remove(m_progressId);
            m_report.Complete();
            OnComplete?.Invoke(m_report);
        }

        private T GetStep<T>()
        {
            foreach (AUploadTask_Step step in m_currentSteps)
            {
                if (step is T t)
                {
                    return t;
                }
            }
            
            return default(T);
        }

        private async Task Execute(AUploadTask_Step[] steps)
        {
            // Do upload steps
            IsSuccessful = true;
            CancellationTokenSource token = new CancellationTokenSource();
            for (int i = 0; i < steps.Length; i++)
            {
                AUploadTask_Step step = steps[i];
                CurrentStep = step;
                CurrentStepType = step.Type;
                if (!IsSuccessful && step.RequiresEverythingBeforeToSucceed)
                {
                    continue;
                }
                
                // Perform the Step (GetSources, CacheSources, etc.)
                m_report.SetProcess(AUploadTask_Step.StepProcess.Intra);
                Task<bool> intraTask;
                try
                {
                    intraTask = step.Run(this, m_report, token);
                }
                catch (Exception ex)
                {
                    UploadTaskReport.StepResult result = m_report.NewReport(step.Type);
                    result.SetFailed($"Failed to complete step {step.Type}. An exception occured!\n\n{ex}");
                    result.AddException(ex);
                    intraTask = Task.FromResult(false);
                }

                while (!intraTask.IsCompleted)
                {
                    float progress = m_report.GetProgress(step.Type, AUploadTask_Step.StepProcess.Intra);
                    SetProgress(i, progress, CurrentStepType.ToString());
                    await Task.Yield();
                }
                bool stepSuccessful = intraTask.Result;
                SetProgress(i, 1f, CurrentStepType.ToString());
                
                // Post-step logic mainly for logging
                m_report.SetProcess(AUploadTask_Step.StepProcess.Post);
                bool postStepSuccessful = false;
                try
                {
                    postStepSuccessful = await step.PostRunResult(this, m_report, stepSuccessful && IsSuccessful);
                }
                catch (Exception ex)
                {
                    UploadTaskReport.StepResult result = m_report.NewReport(step.Type);
                    result.AddException(ex);
                }

                if (!stepSuccessful || !postStepSuccessful || token.IsCancellationRequested)
                {
                    IsSuccessful = false;
                }

                if (step.FireActions)
                {
                    await ExecuteActions(stepSuccessful, UploadConfig.UploadActionData.UploadTrigger.AfterEachStepCompletes);
                }
            }
        }

        public async Task<bool> ExecuteActions(bool valid, 
            UploadConfig.UploadActionData.UploadTrigger trigger)
        {
            UploadTaskReport.StepResult startLog = m_report.NewReport(CurrentStepType);
            startLog.AddLog("Execute Actions with trigger " + trigger + " successful " + valid);
            startLog.SetPercentComplete(1f);
            
            List<UploadConfig.UploadActionData> allActions = m_actions;
            UploadTaskReport.StepResult[] results = m_report.NewReports(CurrentStepType, allActions.Count);
            
            int postActionID = ProgressUtils.Start("Post Upload Actions", "Executing Post Upload Actions...");
            int actionsRun = 0;
            
            for (int i = 0; i < allActions.Count; i++)
            {
                UploadTaskReport.StepResult result = results[i];
                UploadConfig.UploadActionData actionData = allActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    result.SetSkipped($"Skipping post upload action {i+1} because it's null");
                    continue;
                }

                if (!actionData.Triggers.Contains(trigger))
                {
                    result.SetSkipped($"Skipping post upload action {i+1} because it doesn't match the trigger. Trigger: {trigger}");
                    continue;
                }

                UploadConfig.UploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.UploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfSuccessful && !valid) ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfFailed && valid))
                {
                    result.SetSkipped($"Skipping post upload action {i+1} because it doesn't match the current status. Status: {status}. Successful: {valid}");
                    continue;
                }

                result.AddLog($"Executing post upload action: {i+1}");

                actionsRun++;
                bool prepared = await actionData.UploadAction.Prepare(result);
                if (!prepared)
                {
                    result.AddError($"Failed to prepare post upload action: {actionData.UploadAction.GetType().Name}");
                    result.SetPercentComplete(1f);
                    continue;
                }

                try
                {
                    await actionData.UploadAction.Execute(result);
                }
                catch (Exception e)
                {
                    result.AddException(e);
                }
                finally
                {
                    result.SetPercentComplete(1f);
                }
            }

            if (actionsRun == 0)
            {
                UploadTaskReport.StepResult result = m_report.NewReport(CurrentStepType);
                result.SetSkipped("No post upload actions to run under trigger " + trigger + " and successful " + valid);
            }
            
            ProgressUtils.Remove(postActionID);
            return true;
        }

        private void SetProgress(int step, float percent, string message)
        {
            float mainPercent = (float)step / m_totalSteps;
            float subPercent = (1f / m_totalSteps) * Mathf.Clamp01(percent);
            
            PercentComplete = mainPercent + subPercent;
            ProgressUtils.Report(m_progressId, percent, message);
        }

        private void SetupContext(UploadTaskReport report)
        {
            m_context.AddCommand(Wireframe.Context.TASK_FAILED_REASONS_KEY, ()=>
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

            config.Context.SetParent(m_context);
            
            m_uploadConfigs.Add(config);
        }

        public void AddAction(AUploadAction action, 
            UploadConfig.UploadActionData.UploadCompleteStatus whenToExecute = UploadConfig.UploadActionData.UploadCompleteStatus.Always,
            List<UploadConfig.UploadActionData.UploadTrigger> triggers = null)
        {
            if (action == null)
            {
                return;
            }
            
            m_actions.Add(new UploadConfig.UploadActionData(action, whenToExecute, triggers));
        }
        
        public void AddAction(UploadConfig.UploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            m_actions.Add(action);
        }
        
        public void AddActions(List<UploadConfig.UploadActionData> actions)
        {
            if (actions == null)
            {
                return;
            }
            
            m_actions.AddRange(actions.Where(a => a != null));
        }
        
        public void SetBuildDescription(string description)
        {
            m_uploadDescription = description;
        }
        
        internal void SetReport(UploadTaskReport report)
        {
            this.m_report = report;
            m_guid = report.GUID;
            m_uploadName = report.Name;
            IsSuccessful = report.Successful;
            IsComplete = true;
            PercentComplete = 1f;
        }
    }
}