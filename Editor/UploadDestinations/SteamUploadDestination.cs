using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace Wireframe
{
    public class SteamUploadDestination : ASteamBuildDestination
    {
        private bool m_createAppFile = true;
        private bool m_createDepotFile = true;
        private bool m_uploadToSteam = true;

        private SteamBuildConfig m_currentConfig;
        private SteamBuildDepot m_buildDepot;

        private string m_destinationBranch;
        private string m_filePath;
        private string m_unzippedfilePath;
        private bool m_wasBuildSuccessful;
        private SteamBuildConfig m_uploadConfig;
        private SteamBuildDepot m_uploadDepot;


        public SteamUploadDestination(SteamBuildWindow window) : base(window)
        {

        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            // Config
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Config:", GUILayout.Width(120));
                isDirty |= SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref m_currentConfig);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depot:", GUILayout.Width(120));
                isDirty |= SteamBuildWindowUtil.DepotPopup.DrawPopup(m_currentConfig, ref m_buildDepot);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch:", GUILayout.Width(120));
                isDirty |= SteamBuildWindowUtil.BranchPopup.DrawPopup(m_currentConfig, ref m_destinationBranch);
            }

            // Tools
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create AppFile:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createAppFile);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create DepotFile:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createDepotFile);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Upload To Steam:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_uploadToSteam);
            }
        }

        public override void OnGUICollapsed(ref bool isDirty)
        {
            isDirty |= SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref m_currentConfig);
            isDirty |= SteamBuildWindowUtil.DepotPopup.DrawPopup(m_currentConfig, ref m_buildDepot);
            isDirty |= SteamBuildWindowUtil.BranchPopup.DrawPopup(m_currentConfig, ref m_destinationBranch);
        }

        public override async Task Upload(string filePath, string buildDescription)
        {
            m_filePath = filePath;
            m_unzippedfilePath = "";
            m_wasBuildSuccessful = false;
            m_uploadInProgress = true;
            
            m_uploadConfig = new SteamBuildConfig(m_currentConfig);
            m_uploadDepot = new SteamBuildDepot(m_buildDepot);
            
            
            if (File.Exists(filePath) && filePath.EndsWith(".zip"))
            {
                Debug.Log("Unzipping file...");
                m_progressDescription = "Unzipped file...";
                    
                // We need to unzip!
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                m_unzippedfilePath = Application.persistentDataPath + "/ManualBuilds/SteamBuilds/" + fileName;
                
                await Task.Yield(); // Show UI

                if (Directory.Exists(m_unzippedfilePath))
                {
                    Directory.Delete(m_unzippedfilePath);
                }
                
                if (!Directory.Exists(m_unzippedfilePath))
                {
                    Directory.CreateDirectory(m_unzippedfilePath);
                }
                    
                // System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(m_filePath, m_unzippedfilePath);
                }
                catch (IOException e)
                {
                    Debug.LogException(e);
                    m_uploadInProgress = false;
                    return;
                }

                m_filePath = m_unzippedfilePath;
            }

            if (m_createAppFile)
            {
                Debug.Log("Creating new app file");
                m_progressDescription = "Creating App Files";
                m_uploadProgress = 0.25f;
                if (!await SteamSDK.Instance.CreateAppFiles(m_uploadConfig.App, m_uploadDepot.Depot,
                        m_destinationBranch,
                        buildDescription, m_filePath))
                {
                    return;
                }
            }
            else
            {
                Debug.Log("Create App File is disabled. Not creating.");
            }

            if (m_createDepotFile)
            {
                Debug.Log("Creating new depot file");
                m_progressDescription = "Creating Depot File";
                m_uploadProgress = 0.5f;
                if (!await SteamSDK.Instance.CreateDepotFiles(m_uploadDepot.Depot))
                {
                    return;
                }
            }
            else
            {
                Debug.Log("Create Depot File is disabled. Not creating.");
            }

            if (m_uploadToSteam)
            {
                Debug.Log("Uploading to steam. Grab a coffee... this will take a while.");
                m_progressDescription = "Uploading to Steam";
                m_uploadProgress = 0.75f;
                m_wasBuildSuccessful = await SteamSDK.Instance.Upload(m_uploadConfig.App);
            }
            else
            {
                Debug.Log("Upload to Steam is disabled. Not uploading.");
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();
            if (!string.IsNullOrEmpty(m_unzippedfilePath) && Directory.Exists(m_unzippedfilePath))
            {
                Directory.Delete(m_unzippedfilePath);
            }
        }

        public override string ProgressTitle()
        {
            return "Uploading to Steamworks";
        }

        public override bool IsSetup(out string reason)
        {
            if (m_currentConfig == null)
            {
                reason = "Steam Game not selected";
                return false;
            }

            if (m_buildDepot == null)
            {
                reason = "No depot selected";
                return false;
            }

            if (string.IsNullOrEmpty(m_destinationBranch))
            {
                reason = "No branch selected";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool WasUploadSuccessful()
        {
            return m_wasBuildSuccessful;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["m_createAppFile"] = m_createAppFile,
                ["m_createDepotFile"] = m_createDepotFile,
                ["m_uploadToSteam"] = m_uploadToSteam,
                ["m_currentConfig"] = m_currentConfig?.Name,
                ["m_buildDepot"] = m_buildDepot?.Name,
                ["m_destinationBranch"] = m_destinationBranch
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_createAppFile = (bool)data["m_createAppFile"];
            m_createDepotFile = (bool)data["m_createDepotFile"];
            m_uploadToSteam = (bool)data["m_uploadToSteam"];
            m_destinationBranch = (string)data["m_destinationBranch"];

            List<SteamBuildConfig> buildConfigs = SteamBuildWindowUtil.ConfigPopup.GetAllData();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (buildConfigs[i].Name == (string)data["m_currentConfig"])
                {
                    m_currentConfig = buildConfigs[i];
                    for (int j = 0; j < m_currentConfig.Depots.Count; j++)
                    {
                        if (m_currentConfig.Depots[j].Name == (string)data["m_buildDepot"])
                        {
                            m_buildDepot = m_currentConfig.Depots[j];
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}