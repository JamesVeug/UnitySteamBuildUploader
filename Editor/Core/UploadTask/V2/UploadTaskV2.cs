using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// An alternative to <see cref="UploadTask"/> that is not tied to the fixed <see cref="AUploadTask_Step.StepType"/>
    /// pipeline order. Instead it runs a caller-supplied tree of <see cref="AUploadNodeV2"/> nodes
    /// (<see cref="SourceNodeV2"/>/<see cref="DestinationNodeV2"/>/<see cref="ActionNodeV2"/> leaves composed with
    /// <see cref="SequenceNodeV2"/>/<see cref="ParallelNodeV2"/>), so sources/destinations/actions can run in
    /// whatever order and parallelism the caller wires up.
    ///
    /// Validation and Cleanup are NOT part of that tree - they always run first and last respectively, exactly
    /// like the original pipeline: nothing in the tree runs if validation fails, and cleanup always runs
    /// regardless of whether the tree succeeded.
    /// </summary>
    public class UploadTaskV2
    {
        public string GUID { get; }
        public string Name { get; }
        public bool IsSuccessful { get; private set; }
        public bool IsComplete { get; private set; }
        public UploadTaskReport Report { get; }
        public Context Context => m_context;

        public event Action<UploadTaskReport> OnComplete = delegate { };

        // Only exists to satisfy AUploadSource.GetSource(bool, UploadConfig, ...) - UploadTaskV2 doesn't use
        // UploadConfig.Sources/Destinations/PostActions for anything.
        internal UploadConfig DummyConfig { get; } = new UploadConfig();

        private AUploadNodeV2 m_root;
        private readonly List<AUploadSource> m_sources = new List<AUploadSource>();
        private readonly List<AUploadDestination> m_destinations = new List<AUploadDestination>();
        private readonly List<AUploadAction> m_actions = new List<AUploadAction>();
        private readonly List<string> m_allocatedFolders = new List<string>();
        private readonly Context m_context = new Context();
        private int m_sourceFolderCounter;
        private int m_destinationIndexCounter;

        public UploadTaskV2(string name, AUploadNodeV2 root = null)
        {
            GUID = Guid.NewGuid().ToString().Substring(0, 6);
            Name = name;
            m_root = root;
            Report = new UploadTaskReport(GUID, name);
        }

        /// <summary>
        /// Lets the compiler create the task (and its <see cref="Context"/>) before the tree is fully built,
        /// so nodes like SetVariableNode can register a Context command on it while still compiling.
        /// </summary>
        public void SetRoot(AUploadNodeV2 root) => m_root = root;

        public void RegisterLeaf(AUploadSource source) => m_sources.Add(source);
        public void RegisterLeaf(AUploadDestination destination) => m_destinations.Add(destination);
        public void RegisterLeaf(AUploadAction action) => m_actions.Add(action);

        internal string AllocateSourceFolder()
        {
            string folder = Path.Combine(Preferences.CacheFolderPath, "UploadTasksV2", GUID, "source_" + m_sourceFolderCounter++);
            Directory.CreateDirectory(folder);
            m_allocatedFolders.Add(folder);
            return folder;
        }

        internal int NextDestinationIndex() => m_destinationIndexCounter++;

        public void Start()
        {
            _ = StartAsync();
        }

        public async Task StartAsync()
        {
            Report.SetProcess(AUploadTask_Step.StepProcess.Intra);

            foreach (AUploadSource source in m_sources) source.Context.SetParent(m_context);
            foreach (AUploadDestination destination in m_destinations) destination.Context.SetParent(m_context);
            foreach (AUploadAction action in m_actions) action.Context.SetParent(m_context);
            m_context.CacheCallbacks();

            CancellationTokenSource token = new CancellationTokenSource();

            UploadTaskReport.StepResult validationResult = Report.NewReport(AUploadTask_Step.StepType.Validation);
            List<GUIContent> errors = new List<GUIContent>();
            foreach (AUploadSource source in m_sources) source.TryGetErrors(errors);
            foreach (AUploadDestination destination in m_destinations) destination.TryGetErrors(errors);
            foreach (AUploadAction action in m_actions) action.TryGetErrors(errors);

            bool valid = errors.Count == 0;
            foreach (GUIContent error in errors)
            {
                validationResult.SetFailed(string.IsNullOrEmpty(error.tooltip) ? error.text : error.text + ": " + error.tooltip);
            }
            validationResult.SetPercentComplete(1f);

            if (valid)
            {
                try
                {
                    IsSuccessful = m_root != null && await m_root.Run(this, Report, token);
                }
                catch (Exception e)
                {
                    UploadTaskReport.StepResult result = Report.NewReport(AUploadTask_Step.StepType.Upload);
                    result.AddException(e);
                    result.SetFailed("Upload task failed - " + e.Message);
                    IsSuccessful = false;
                }
            }
            else
            {
                IsSuccessful = false;
            }

            await RunCleanup();

            IsComplete = true;
            Report.Complete();
            OnComplete(Report);
        }

        private async Task RunCleanup()
        {
            UploadTaskReport.StepResult cleanupResult = Report.NewReport(AUploadTask_Step.StepType.Cleanup);

            foreach (AUploadSource source in m_sources)
            {
                try
                {
                    await source.CleanUp(0, cleanupResult);
                }
                catch (Exception e)
                {
                    cleanupResult.AddException(e);
                }
            }

            foreach (AUploadDestination destination in m_destinations)
            {
                try
                {
                    await destination.CleanUp(cleanupResult);
                }
                catch (Exception e)
                {
                    cleanupResult.AddException(e);
                }
            }

            foreach (AUploadAction action in m_actions)
            {
                try
                {
                    await action.CleanUp(cleanupResult);
                }
                catch (Exception e)
                {
                    cleanupResult.AddException(e);
                }
            }

            if (Preferences.DeleteCacheAfterUpload)
            {
                foreach (string folder in m_allocatedFolders)
                {
                    try
                    {
                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, true);
                        }
                    }
                    catch (Exception e)
                    {
                        cleanupResult.AddException(e);
                    }
                }
            }

            cleanupResult.SetPercentComplete(1f);
        }
    }
}
