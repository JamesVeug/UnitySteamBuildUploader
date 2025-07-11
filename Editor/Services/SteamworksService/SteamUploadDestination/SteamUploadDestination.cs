using System.Collections.Generic;
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
    [BuildDestination("Steamworks")]
    public partial class SteamUploadDestination : ABuildDestination
    {
        [Wiki("App", "Which Steam App to upload to. eg: 1141030", 1)]
        private SteamApp m_current;
        
        [Wiki("Depot", "Which Depot to upload to. eg: 1141031", 1)]
        private SteamDepot m_depot;
        
        [Wiki("Branch", "Which Branch to upload to. eg: internal-testing", 1)]
        private SteamBranch m_destinationBranch;
        
        [Wiki("Create AppFile", "If true, a new App File is creating to upload the build to Steam.", 2)]
        private bool m_createAppFile = true;
        
        [Wiki("Create DepotFile", "If true, a new Depot File is creating to upload the build to Steam.", 2)]
        private bool m_createDepotFile = true;
        
        [Wiki("Upload to Steam", "If true, the build will be uploaded to Steam. Otherwise will still attempt to login. Good for testing if you can login correctly.", 2)]
        private bool m_uploadToSteam = true;
        
        private SteamApp m_uploadApp;
        private SteamDepot m_uploadDepot;
        private SteamBranch m_uploadBranch;

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
        
        public void SetFlags(bool createAppFile = true, bool createDepotFile = true, bool uploadToSteam = true)
        {
            m_createAppFile = createAppFile;
            m_createDepotFile = createDepotFile;
            m_uploadToSteam = uploadToSteam;
        }

        public override async Task<bool> Prepare(string filePath, string buildDescription, BuildTaskReport.StepResult result)
        {
            await base.Prepare(filePath, buildDescription, result);

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

            if (m_createAppFile)
            {
                result.AddLog("Creating new app file");
                m_uploadProgress = 0.25f;
                if (!await SteamSDK.Instance.CreateAppFiles(m_uploadApp.App, m_uploadDepot.Depot,
                        m_uploadBranch.name, buildDescription, m_filePath, result))
                {
                    result.SetFailed("Failed to create app file");
                    return false;
                }
            }
            else
            {
                result.AddLog("Create App File is disabled. Not creating.");
            }

            if (m_createDepotFile)
            {
                result.AddLog("Creating new depot file");
                m_uploadProgress = 0.5f;
                if (!await SteamSDK.Instance.CreateDepotFiles(m_uploadDepot.Depot, m_uploadBranch.name, result))
                {
                    result.SetFailed("Failed to create depot file");
                    return false;
                }
            }
            else
            {
                result.AddLog("Create Depot File is disabled. Not creating.");
            }

            return true;
        }

        public override async Task<bool> Upload(BuildTaskReport.StepResult result)
        {
            m_uploadProgress = 0.75f;

            return await SteamSDK.Instance.Upload(m_uploadApp.App, m_uploadToSteam, result);
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["m_createAppFile"] = m_createAppFile,
                ["m_createDepotFile"] = m_createDepotFile,
                ["m_uploadToSteam"] = m_uploadToSteam,
                ["configID"] = m_current?.Id,
                ["depotID"] = m_depot?.Id,
                ["branchID"] = m_destinationBranch?.Id
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_createAppFile = (bool)data["m_createAppFile"];
            m_createDepotFile = (bool)data["m_createDepotFile"];
            m_uploadToSteam = (bool)data["m_uploadToSteam"];
            
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
            if(data.TryGetValue("depotID", out object depotIDString) && depotIDString != null)
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
        }

        public override void TryGetWarnings(List<string> warnings)
        {
            base.TryGetWarnings(warnings);

            if (!m_uploadToSteam)
            {
                warnings.Add("Uploading to Steam is disabled but the Build Uploader will still create required and log into Steam.");
            }
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (!InternalUtils.GetService<SteamworksService>().IsReadyToStartBuild(out string serviceReason))
            {
                errors.Add(serviceReason);
            }
            
            if (m_current == null)
            {
                errors.Add("No App selected");
            }

            if (m_depot == null)
            {
                errors.Add("No Depot selected");
            }

            if (m_destinationBranch == null)
            {
                errors.Add("No Branch selected");
            }
            else if (m_destinationBranch.name == "default")
            {
                errors.Add("Uploading to the 'default' branch is not allowed by the SteamSDK.\nUse none or an empty branch name instead and use the Steamworks dashboard to assign to default.");
            }
        }
    }
}