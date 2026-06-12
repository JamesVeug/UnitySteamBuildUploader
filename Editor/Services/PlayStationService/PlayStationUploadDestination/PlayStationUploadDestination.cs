using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Upload a build to PlayStation Partners using the local PlayStation SDK publishing tool.
    ///
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Experimental]
    [Wiki("PlayStation", "destinations", "Uploads files to PlayStation Partners via the local PlayStation SDK publishing tool")]
    [UploadDestination("PlayStation")]
    public partial class PlayStationUploadDestination : AUploadDestination
    {
        [Wiki("Title", "Which PlayStation Title to upload to.", 1)]
        private PlayStationApp m_app;

        [Wiki("Branch", "Which Branch / release ring to upload to. eg: internal", 2)]
        private PlayStationBranch m_destinationBranch;

        [Wiki("Description Format", "Build description that appears in PlayStation Partners.", 9)]
        private string m_descriptionFormat = Context.TASK_DESCRIPTION_KEY;

        private PlayStationApp m_uploadApp;
        private PlayStationBranch m_uploadBranch;
        private string m_appPath;

        public PlayStationUploadDestination() : base()
        {
            // Required for reflection
        }

        public PlayStationUploadDestination(string titleId, string contentId, string branchName) : base()
        {
            SetPlayStationApp(titleId, contentId);
            SetPlayStationBranch(branchName);
        }

        public void SetPlayStationApp(string titleId, string contentId)
        {
            m_app = new PlayStationApp()
            {
                TitleID = titleId,
                ContentID = contentId
            };
        }

        public void SetPlayStationBranch(string branchName)
        {
            m_destinationBranch = new PlayStationBranch(branchName);
        }

        public override async Task<bool> Prepare(string taskGUID, int configIndex, int destinationIndex,
            string taskContentsFolder, UploadTaskReport.StepResult result)
        {
            await base.Prepare(taskGUID, configIndex, destinationIndex, taskContentsFolder, result);

            if (m_app == null)
            {
                result.SetFailed("No Title selected");
                return false;
            }

            if (m_destinationBranch == null)
            {
                result.SetFailed("No Branch selected");
                return false;
            }

            string buildDescription = m_context.FormatString(m_descriptionFormat);
            string suffix = $"buildUploader_{taskGUID}_{configIndex}_{destinationIndex}";

            result.AddLog("Creating new PlayStation authoring file");
            m_uploadApp = new PlayStationApp(m_app);
            m_uploadBranch = new PlayStationBranch(m_destinationBranch);

            string appFiles = await PlayStationSDK.Instance.CreateAppFiles(m_uploadApp, m_uploadBranch, buildDescription,
                m_taskContentsFolder, result, suffix);
            if (string.IsNullOrEmpty(appFiles))
            {
                // NOTE: SetFailed called in CreateAppFiles
                return false;
            }
            m_appPath = appFiles;

            if (string.IsNullOrEmpty(m_appPath) || !File.Exists(m_appPath))
            {
                result.SetFailed("Failed to create authoring file or authoring file does not exist: " + m_appPath);
                return false;
            }

            return true;
        }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result)
        {
            return await PlayStationSDK.Instance.Upload(m_uploadApp, m_uploadBranch, m_appPath, result);
        }

        public override Task CleanUp(UploadTaskReport.StepResult stepResult)
        {
            base.CleanUp(stepResult);

            m_uploadApp = null;
            m_uploadBranch = null;

            if (PlayStationService.DeleteAuthoringFilesDuringCleanup)
            {
                if (!string.IsNullOrEmpty(m_appPath) && File.Exists(m_appPath))
                {
                    stepResult.AddLog("Deleting authoring file: " + m_appPath);
                    File.Delete(m_appPath);
                }
            }
            else
            {
                stepResult.AddLog("Skipping deletion of PlayStation authoring file as per preferences.");
            }
            m_appPath = null;

            return Task.CompletedTask;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["configID"] = m_app?.Id,
                ["branchID"] = m_destinationBranch?.Id,
                ["m_descriptionFormat"] = m_descriptionFormat
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            // Title
            PlayStationApp[] buildConfigs = PlayStationUIUtils.ConfigPopup.Values;
            if (data.TryGetValue("configID", out object configIDString) && configIDString != null && configIDString is long configID)
            {
                m_app = buildConfigs.FirstOrDefault(a => a.Id == configID);
            }

            if (m_app == null)
            {
                return;
            }

            // Branch
            if (data.TryGetValue("branchID", out object branchIDString) && branchIDString != null)
            {
                m_destinationBranch = m_app.ConfigBranches.FirstOrDefault(a => a.Id == (long)branchIDString);
            }

            // Description Format
            if (data.TryGetValue("m_descriptionFormat", out object descriptionFormatObj) && descriptionFormatObj != null)
            {
                m_descriptionFormat = descriptionFormatObj.ToString();
            }
            else
            {
                m_descriptionFormat = Context.TASK_DESCRIPTION_KEY;
            }
        }

        public override void TryGetWarnings(List<GUIContent> warnings, Context ctx)
        {
            base.TryGetWarnings(warnings, ctx);
        }

        public override void TryGetErrors(List<GUIContent> errors)
        {
            base.TryGetErrors(errors);

            PlayStationService service = InternalUtils.GetService<PlayStationService>();
            if (!service.IsReadyToStartBuild(out GUIContent serviceReason))
            {
                errors.Add(serviceReason);
            }

            if (m_app == null)
            {
                errors.Add(new GUIContent("No Title selected"));
            }
            else
            {
                if (string.IsNullOrEmpty(m_app.TitleID))
                {
                    errors.Add(service.ProjectSettingsLink($"PlayStation Title '{m_app.Name}' does not have a Title ID set.", ""));
                }

                if (string.IsNullOrEmpty(m_app.ContentID))
                {
                    errors.Add(service.ProjectSettingsLink($"PlayStation Title '{m_app.Name}' does not have a Content ID set.", ""));
                }
            }

            if (m_destinationBranch == null)
            {
                errors.Add(new GUIContent("No Branch selected"));
            }

            if (string.IsNullOrEmpty(m_descriptionFormat))
            {
                errors.Add(new GUIContent("No build description specified."));
            }
        }
    }
}
