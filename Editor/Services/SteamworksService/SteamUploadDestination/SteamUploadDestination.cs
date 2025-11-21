using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Upload a build to Steamworks
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki("Steamworks", "destinations", "Uploads files to Steamworks")]
    [UploadDestination("Steamworks")]
    public partial class SteamUploadDestination : AUploadDestination
    {
        [Wiki("App", "Which Steam App to upload to. eg: 1141030", 1)]
        private SteamApp m_current;
        
        [Wiki("Depot", "Which Depot to upload to. eg: 1141031", 1)]
        private SteamDepot m_depot;
        
        [Wiki("Branch", "Which Branch to upload to. eg: internal-testing", 1)]
        private SteamBranch m_destinationBranch;
        
        [Wiki("Create AppFile", "If true, a new App File is creating to upload the build to Steam.", 2)]
        private bool m_createAppFile = true;
        
        [Wiki("AppFile Path", "If Create AppFile is false then use a file with this name that will be found in the SteamSDKs path to upload a build to Steam.", 3)]
        private string m_appFileName = "";
        
        [Wiki("Overwrite AppFile Description", "If Create AppFile is false and this is true, the the chosen appFile will be copied and description changed to fit selected Build Uploader description.", 4)]
        private bool m_appFileOverwriteDesc = true;
        
        [Wiki("Create DepotFile", "If true, a new Depot File is creating to upload the build to Steam.", 5)]
        private bool m_createDepotFile = true;
        
        [Wiki("DepotFile Path", "If Create DepotFile is false then use a file with this name that  will be found in the SteamSDKs path to upload a build to Steam.", 6)]
        private string m_depotFileName = "";
        
        [Wiki("Description Format", "What description to upload to steam to appear on steamworks.", 7)]
        private string m_descriptionFormat = StringFormatter.TASK_DESCRIPTION_KEY;
        
        private SteamApp m_uploadApp;
        private SteamDepot m_uploadDepot;
        private SteamBranch m_uploadBranch;
        private string m_appPath;
        private string m_depotPath;

        public SteamUploadDestination() : base()
        {
            // Required for reflection
        }
        
        public SteamUploadDestination(int appID, int depotID, string branchName) : base()
        {
            SetSteamApp(appID);
            SetSteamDepot(depotID);
            SetSteamBranch(branchName);
        }
        
        public void SetSteamApp(int appID)
        {
            m_current = new SteamApp()
            {
                App = new AppVDFFile()
                {
                    appid = appID
                }
            };
        }
        
        public void SetSteamDepot(int depotID)
        {
            m_depot = new SteamDepot()
            {
                Depot = new DepotVDFFile()
                {
                    DepotID = depotID
                }
            };
        }
        
        public void SetSteamBranch(string branchName)
        {
            m_destinationBranch = new SteamBranch(branchName);
        }
        
        public void SetFlags(bool createAppFile = true, bool createDepotFile = true)
        {
            m_createAppFile = createAppFile;
            m_createDepotFile = createDepotFile;
        }

        public override async Task<bool> Prepare(string taskGUID, int configIndex, int destinationIndex,
            string cachedFolderPath, UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            await base.Prepare(taskGUID, configIndex, destinationIndex, cachedFolderPath, result, ctx);

            if (m_current == null)
            {
                result.SetFailed("No App selected");
                return false;
            }
            
            if (m_depot == null)
            {
                result.SetFailed("No Depot selected");
                return false;
            }
            
            if (m_destinationBranch == null)
            {
                result.SetFailed("No Branch selected");
                return false;
            }
            
            m_uploadApp = new SteamApp(m_current);
            m_uploadDepot = new SteamDepot(m_depot);
            m_uploadBranch = new SteamBranch(m_destinationBranch);

            string buildDescription = StringFormatter.FormatString(m_descriptionFormat, ctx);
            string suffix = $"buildUploader_{taskGUID}_{configIndex}_{destinationIndex}";
            if (m_createAppFile)
            {
                m_appPath = await SteamSDK.Instance.CreateAppFiles(m_uploadApp.App, m_uploadDepot.Depot, m_uploadBranch.name, buildDescription, m_cachedFolderPath, result, suffix); 
                result.AddLog("Created new app file: " + m_appPath);
            }
            else
            {
                // Use the provided app file name
                string[] files = GetVDFFile(m_appFileName, ctx);
                if (files.Length == 0)
                {
                    result.SetFailed("App file not found: " + m_appFileName);
                    return false;
                }
                else if (files.Length > 1)
                {
                    result.SetFailed("Multiple App files found with name: " + m_appFileName + ". Please specify a unique App File name.");
                    return false;
                }
                
                m_appPath = files[0];

                if (m_appFileOverwriteDesc)
                {
                    if (!SteamSDK.TryCopyAppFileAndModifyDescAtPath(m_appPath, out m_appPath, buildDescription, result))
                    {
                        return false;
                    }
                }
                
            }
            
            if (string.IsNullOrEmpty(m_appPath) || !File.Exists(m_appPath))
            {
                result.SetFailed("Failed to create app file or app file does not exist: " + m_appPath);
                return false;
            }

            if (m_createDepotFile)
            {
                result.AddLog("Creating new depot file");
                m_depotPath = await SteamSDK.Instance.CreateDepotFiles(m_uploadDepot.Depot, m_uploadBranch.name, result, suffix);
            }
            else
            {
                // Use the provided depot file name
                string[] files = GetVDFFile(m_depotFileName, ctx);
                if (files.Length == 0)
                {
                    result.SetFailed("Depot file not found: " + m_depotFileName);
                    return false;
                }
                else if (files.Length > 1)
                {
                    result.SetFailed("Multiple Depot files found with name: " + m_depotFileName + ". Please specify a unique Depot File name.");
                    return false;
                }
                
                m_depotPath = files[0];
            }
            
            if (string.IsNullOrEmpty(m_depotPath) || !File.Exists(m_depotPath))
            {
                result.SetFailed("Failed to create depot file or depot file does not exist: " + m_depotPath);
                return false;
            }

            return true;
        }

        private string[] GetVDFFile(string fileName, StringFormatter.Context ctx)
        {
            string appFileName = fileName;
            if (!appFileName.EndsWith(".vdf", StringComparison.OrdinalIgnoreCase))
            {
                appFileName += ".vdf"; // Ensure it has .vdf extension
            }
            appFileName = StringFormatter.FormatString(appFileName, ctx);

            if (Path.IsPathRooted(appFileName))
            {
                // NOTE: At this time i only support specifying files that are in the Scripts folder
                // TODO: Copy the selected file over to the scripts folder or modify the .vdfs to use absolute paths
                appFileName = Path.GetFileName(appFileName);
            }
            
            return Directory.GetFiles(SteamSDK.SteamScriptPath, appFileName, SearchOption.AllDirectories);
        }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            return await SteamSDK.Instance.Upload(m_uploadApp.App, m_appPath, result);
        }

        public override Task CleanUp(UploadTaskReport.StepResult stepResult)
        {
            base.CleanUp(stepResult);
            
            m_uploadApp = null;
            m_uploadDepot = null;
            m_uploadBranch = null;
            
            if (m_createAppFile && !string.IsNullOrEmpty(m_appPath))
            {
                stepResult.AddLog("Deleting app file: " + m_appPath);
                if (File.Exists(m_appPath))
                {
                    File.Delete(m_appPath);
                }

            }
            m_appPath = null;
            
            if (m_createDepotFile && !string.IsNullOrEmpty(m_depotPath))
            {
                stepResult.AddLog("Deleting depot file: " + m_depotPath);
                if (File.Exists(m_depotPath))
                {
                    File.Delete(m_depotPath);
                }
            }
            m_depotPath = null;
            
            return Task.CompletedTask;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["m_createAppFile"] = m_createAppFile,
                ["m_appFileName"] = m_appFileName,
                ["m_appFileOverwriteDesc"] = m_appFileOverwriteDesc,
                ["m_createDepotFile"] = m_createDepotFile,
                ["m_depotFileName"] = m_depotFileName,
                ["configID"] = m_current?.Id,
                ["depotID"] = m_depot?.Id,
                ["branchID"] = m_destinationBranch?.Id,
                ["m_descriptionFormat"] = m_descriptionFormat
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_createAppFile = (bool)data["m_createAppFile"];
            if (data.TryGetValue("m_appFileName", out object appFileNameObj) && appFileNameObj != null)
            {
                m_appFileName = appFileNameObj.ToString();
            }
            else
            {
                m_appFileName = "";
            }
            
            m_createDepotFile = (bool)data["m_createDepotFile"];
            if (data.TryGetValue("m_depotFileName", out object depotFileNameObj) && depotFileNameObj != null)
            {
                m_depotFileName = depotFileNameObj.ToString();
            }
            else
            {
                m_depotFileName = "";
            }
            
            if (data.TryGetValue("m_appFileOverwriteDesc", out object appFileOverwriteDescObj) && appFileOverwriteDescObj != null)
            {
                m_appFileOverwriteDesc = appFileOverwriteDescObj is bool b && b;
            }
            else
            {
                m_appFileOverwriteDesc = true;
            }
            
            // Note: In 1.2.2 the serialization data was changed from the Name to ID
            
            // Config
            SteamApp[] buildConfigs = SteamUIUtils.ConfigPopup.Values;
            if (data.TryGetValue("configID", out object configIDString) && configIDString != null)
            {
                m_current = buildConfigs.FirstOrDefault(a=> a.Id == (long)configIDString);
            }
            else if (data.TryGetValue("m_currentConfig", out object m_currentConfigName))
            {
                m_current = buildConfigs.FirstOrDefault(a=>a.Name == m_currentConfigName.ToString());
            }

            if (m_current == null)
            {
                // No config found so don't continue.
                return;
            }
            
            // Depot
            if(data.TryGetValue("depotID", out var depotIDString) && depotIDString != null)
            {
                m_depot = m_current.Depots.FirstOrDefault(a=>a.Id == (long)depotIDString);
            }
            else if (data.TryGetValue("m_buildDepot", out object m_buildDepotName))
            {
                m_depot = m_current.Depots.FirstOrDefault(a=>a.Name == m_buildDepotName.ToString());
            }
            
            // Branch
            if (data.TryGetValue("branchID", out object branchIDString) && branchIDString != null)
            {
                m_destinationBranch = m_current.ConfigBranches.FirstOrDefault(a=>a.Id == (long)branchIDString);
            }
            else if (data.TryGetValue("m_destinationBranch", out object m_destinationBranchName))
            {
                m_destinationBranch = m_current.ConfigBranches.FirstOrDefault(a=>a.name == m_destinationBranchName.ToString());
            }
            
            // Build Description Format - Added in v3.1.0
            if (data.TryGetValue("m_descriptionFormat", out object descriptionFormatObj) && descriptionFormatObj != null)
            {
                m_descriptionFormat = descriptionFormatObj.ToString();
            }
            else
            {
                m_descriptionFormat = StringFormatter.TASK_DESCRIPTION_KEY;
            }
        }

        public override void TryGetWarnings(List<string> warnings, StringFormatter.Context ctx)
        {
            base.TryGetWarnings(warnings, ctx);
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (!InternalUtils.GetService<SteamworksService>().IsReadyToStartBuild(out string serviceReason))
            {
                errors.Add(serviceReason);
            }
            
            if (m_current == null)
            {
                errors.Add("No App selected");
            }
            
            if(!m_createAppFile)
            {
                if (string.IsNullOrEmpty(m_appFileName))
                {
                    errors.Add("No App File name specified. Either create a new App File or specify an existing App File name.");
                }
                else
                {
                    string[] appFiles = GetVDFFile(m_appFileName, ctx);
                    if (appFiles.Length == 0)
                    {
                        errors.Add("App File '" + m_appFileName + "' not found in path '" + SteamSDK.SteamScriptPath + "'!");
                    }
                    else if(appFiles.Length > 1)
                    {
                        errors.Add("Multiple App Files found with name: '" + m_appFileName + "'. Please specify a unique App File name.");
                    }
                }
            }

            if (m_depot == null)
            {
                errors.Add("No Depot selected");
            }
            
            if(!m_createDepotFile)
            {
                if (string.IsNullOrEmpty(m_depotFileName))
                {
                    errors.Add("No Depot File name specified. Either create a new Depot File or specify an existing Depot File name.");
                }
                else
                {
                    string[] depotFiles = GetVDFFile(m_depotFileName, ctx);
                    if (depotFiles.Length == 0)
                    {
                        errors.Add("Depot File '" + m_depotFileName + "' not found in path '" + SteamSDK.SteamScriptPath + "'!");
                    }
                    else if(depotFiles.Length > 1)
                    {
                        errors.Add("Multiple Depot Files found with name: " + m_depotFileName + ". Please specify a unique Depot File name.");
                    }
                }
            }

            if (m_destinationBranch == null)
            {
                errors.Add("No Branch selected");
            }
            else if (m_destinationBranch.name == "default")
            {
                errors.Add("Uploading to the 'default' branch is not allowed by the SteamSDK.\nUse none or an empty branch name instead and use the Steamworks dashboard to assign to default.");
            }

            if (string.IsNullOrEmpty(m_descriptionFormat))
            {
                errors.Add("No build description specified.");
            }
        }
    }
}