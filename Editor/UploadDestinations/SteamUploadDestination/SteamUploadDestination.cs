using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace Wireframe
{
    internal class SteamUploadDestination : ABuildDestination
    {
        private bool m_createAppFile = true;
        private bool m_createDepotFile = true;
        private bool m_uploadToSteam = true;

        private SteamApp _mCurrent;
        private SteamDepot _mDepot;
        private SteamBranch m_destinationBranch;
        
        private string m_filePath;
        private string m_unzippedfilePath;
        private bool m_wasBuildSuccessful;
        private SteamApp _mUpload;
        private SteamDepot m_uploadDepot;
        private SteamBranch m_uploadBranch;


        public SteamUploadDestination(BuildUploaderWindow uploaderWindow) : base(uploaderWindow)
        {

        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            // Config
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Config:", GUILayout.Width(120));
                isDirty |= UIUtils.ConfigPopup.DrawPopup(ref _mCurrent);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depot:", GUILayout.Width(120));
                isDirty |= UIUtils.DepotPopup.DrawPopup(_mCurrent, ref _mDepot);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch:", GUILayout.Width(120));
                isDirty |= UIUtils.BranchPopup.DrawPopup(_mCurrent, ref m_destinationBranch);
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
            isDirty |= UIUtils.ConfigPopup.DrawPopup(ref _mCurrent);
            isDirty |= UIUtils.DepotPopup.DrawPopup(_mCurrent, ref _mDepot);
            isDirty |= UIUtils.BranchPopup.DrawPopup(_mCurrent, ref m_destinationBranch);
        }

        public override async Task<bool> Upload(string filePath, string buildDescription)
        {
            m_filePath = filePath;
            m_unzippedfilePath = "";
            m_wasBuildSuccessful = false;
            m_uploadInProgress = true;
            
            _mUpload = new SteamApp(_mCurrent);
            m_uploadDepot = new SteamDepot(_mDepot);
            m_uploadBranch = new SteamBranch(m_destinationBranch);
            
            
            if (File.Exists(filePath) && filePath.EndsWith(".zip"))
            {
                Debug.Log("Unzipping file...");
                m_progressDescription = "Unzipped file...";
                    
                // We need to unzip!
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                m_unzippedfilePath = Application.persistentDataPath + "/BuildUploader/CachedBuilds/SteamBuilds/" + fileName;
                
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
                    return false;
                }

                m_filePath = m_unzippedfilePath;
            }

            if (m_createAppFile)
            {
                Debug.Log("Creating new app file");
                m_progressDescription = "Creating App Files";
                m_uploadProgress = 0.25f;
                if (!await SteamSDK.Instance.CreateAppFiles(_mUpload.App, m_uploadDepot.Depot,
                        m_destinationBranch.name,
                        buildDescription, m_filePath))
                {
                    return false;
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
                    return false;
                }
            }
            else
            {
                Debug.Log("Create Depot File is disabled. Not creating.");
            }

            Debug.Log("Uploading to steam. Grab a coffee... this will take a while.");
            m_progressDescription = "Uploading to Steam";
            m_uploadProgress = 0.75f;
            m_wasBuildSuccessful = await SteamSDK.Instance.Upload(_mUpload.App, m_uploadToSteam);

            return m_wasBuildSuccessful;
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
            if (_mCurrent == null)
            {
                reason = "Steam Game not selected";
                return false;
            }

            if (_mDepot == null)
            {
                reason = "No depot selected";
                return false;
            }

            if (m_destinationBranch == null)
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
                ["configID"] = _mCurrent?.Id,
                ["depotID"] = _mDepot?.Id,
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
            List<SteamApp> buildConfigs = UIUtils.ConfigPopup.GetAllData();
            if (data.TryGetValue("configID", out object configIDString) && configIDString != null)
            {
                _mCurrent = buildConfigs.FirstOrDefault(a=> a.Id == (long)configIDString);
            }
            else if (data.TryGetValue("m_currentConfig", out object m_currentConfigName))
            {
                _mCurrent = buildConfigs.FirstOrDefault(a=>a.Name == m_currentConfigName.ToString());
            }

            if (_mCurrent == null)
            {
                // No config found so don't continue.
                return;
            }
            
            // Depot
            if(data.TryGetValue("depotID", out object depotIDString))
            {
                _mDepot = _mCurrent.Depots.FirstOrDefault(a=>a.Id == (long)depotIDString);
            }
            else if (data.TryGetValue("m_buildDepot", out object m_buildDepotName))
            {
                _mDepot = _mCurrent.Depots.FirstOrDefault(a=>a.Name == m_buildDepotName.ToString());
            }
            
            // Branch
            if (data.TryGetValue("branchID", out object branchIDString))
            {
                m_destinationBranch = _mCurrent.ConfigBranches.FirstOrDefault(a=>a.Id == (long)branchIDString);
            }
            else if (data.TryGetValue("m_destinationBranch", out object m_destinationBranchName))
            {
                m_destinationBranch = _mCurrent.ConfigBranches.FirstOrDefault(a=>a.name == m_destinationBranchName.ToString());
            }
        }
    }
}