using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    [Wiki("EpicGames", "destinations", "Upload an artifact to Epic Games.")]
    [UploadDestination("EpicGames")]
    public partial class EpicGamesUploadDestination : AUploadDestination
    {
        [Wiki("Organization", "Use the Organization string that was provided along with your credentials.", 0)]
        private EpicGamesOrganization Organization;
        
        [Wiki("ProductID", "Use the Product/Game string that was provided along with your credentials.", 1)]
        private EpicGamesProduct Product;
        
        [Wiki("Artifact", "Specify the Artifact string that was provided along with your credentials.", 2)]
        private EpicGamesArtifact Artifact;
        
        [Wiki("Cloud Directory Override", "Optional: Directory where BuildPatchTool can save files to be uploaded, this can be empty each run. As with the BuildRoot, this can be an absolute or a relative path. (This location is used to cache information about existing binaries, and should be a different directory from the BuildRoot parameter. It is OK if this directory is initially empty; BuildPatchTool will download information as needed from the Epic backend and store it in the CloudDir.)", 5)]
        private string CloudDirOverride;
        
        [Wiki("BuildVersion", "The version string for the build. This needs to be unique for each build of a specific artifact, independent of platform. For example, BuildVersion-1.0 can only exists for Windows or Mac, not both. The build version string has the following restrictions: Must be between 1 and 100 chars in length, whitespace is not allowed, should only contain characters from the following sets a-z, A-Z, 0-9, or .+-_", 6)]
        private string BuildVersion = "v{version}_{buildtarget}_b{buildnumber}";
        
        [Wiki("AppLaunch", "The path to the app executable that should be launched when running your game, relative to (and inside of) the BuildRoot. For Mac binaries, this should be the executable file contained within the .app folder, usually in the location Game.app/Contents/MacOS/Game.", 7)]
        private string AppLaunch;
        
        [Wiki("AppArgs", "The commandline to send to the app on launch. This can be set to “” when no additional arguments are needed.", 8)]
        private string AppArgs = "";

        public EpicGamesUploadDestination() : base() { }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result)
        {
            string secret = GetSecret();
            string cloudDir = GetCloudDir();
            
            result.AddLog("Starting Epic Games upload...");
            return await EpicGames.Upload(result, m_context,
                m_cachedFolderPath,
                Organization.OrganizationID,
                Product.ProductID,
                Artifact.ArtifactID,
                Product.ClientID,
                cloudDir,
                BuildVersion,
                AppLaunch,
                Product.SecretType,
                secret,
                AppArgs);
        }

        private string GetCloudDir()
        {
            string cloudDir = "";
            if (!string.IsNullOrEmpty(CloudDirOverride))
            {
                cloudDir = m_context.FormatString(CloudDirOverride);
            }
            else
            {
                cloudDir = m_context.FormatString(EpicGames.CloudPath);
            }

            return cloudDir;
        }

        private string GetSecret()
        {
            switch (Product.SecretType)
            {
                case EpicGamesProduct.SecretTypes.EnvVar:
                    return m_context.FormatString(Product.ClientSecretEnvVar);
                case EpicGamesProduct.SecretTypes.ClientSecret:
                    return EpicGamesService.GetClientSecret(Organization.OrganizationID, Product.ClientID);
            }

            return "";
        }

        public override void TryGetErrors(List<string> errors)
        {
            if (string.IsNullOrEmpty(EpicGames.SDKPath))
            {
                errors.Add("SDK Path not set in Preferences");
            }
            
            if (Organization == null)
            {
                errors.Add("Organization is not set");
            }

            if (Product == null)
            {
                errors.Add("Product is not set");
            }

            if (Artifact == null)
            {
                errors.Add("Artifact is not set");
            }

            if (string.IsNullOrEmpty(AppLaunch))
            {
                errors.Add("App Launch is not set");
            }

            if (string.IsNullOrEmpty(BuildVersion))
            {
                errors.Add("Build Version not set");
            }
            else if (!ValidateBuildVersion())
            {
                errors.Add("Invalid Build Version: Should only contain characters from the following sets a-z, A-Z, 0-9, or .+-_");
            }
            
            if (Utils.PathContainsInvalidCharacters(GetCloudDir()))
            {
                errors.Add("Cloud Directory contains invalid characters: '" + GetCloudDir() + "'");
            }

            if (Product != null && Organization != null)
            {
                if (string.IsNullOrEmpty(GetSecret()))
                {
                    errors.Add("Client Secret is not set");
                }
            }
        }

        private static readonly char[] buildVersionValidCharacters = "abcdefghijklmnopqurstuvwxyz0123456789.+-_".ToCharArray();
        private bool ValidateBuildVersion()
        {
            string formattedBuildVersion = m_context.FormatString(BuildVersion).Trim();
            if (string.IsNullOrEmpty(formattedBuildVersion))
            {
                return false;
            }
            
            foreach (char c in formattedBuildVersion)
            {
                if (!buildVersionValidCharacters.Contains(char.ToLower(c)))
                {
                    return false;
                }
            }

            return true;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrganizationID"] = Organization?.ID;
            dict["ProductID"] = Product?.ID;
            dict["ArtifactID"] = Artifact?.ID;
            dict["CloudDirOverride"] = CloudDirOverride;
            dict["BuildVersion"] = BuildVersion;
            dict["AppLaunch"] = AppLaunch;
            dict["AppArgs"] = AppArgs;
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            EpicGamesOrganization[] organizations = EpicGamesUIUtils.OrganizationPopup.Values;
            if (data.TryGetValue("OrganizationID", out object orgID) && orgID != null)
            {
                Organization = organizations.FirstOrDefault(a=> a.ID == (long)orgID);
                if (Organization == null)
                {
                    Debug.LogError("Could not get Organization: " + orgID);
                }
            }

            if (Organization != null && data.TryGetValue("ProductID", out object productID) && productID != null)
            {
                Product = Organization.Products.FirstOrDefault(a => a.ID == (long)productID);
                if (Product != null)
                {
                    if (data.TryGetValue("ArtifactID", out object artifactID) && artifactID != null)
                    {
                        Artifact = Product.Artifacts.FirstOrDefault(a=> a.ID == (long)artifactID);
                        if (Artifact == null)
                        {
                            Debug.LogError("Could not get Artifact: " + artifactID);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Could not get Product: " + productID);
                }
            }
            
            CloudDirOverride = (string)data["CloudDirOverride"];
            BuildVersion = (string)data["BuildVersion"];
            AppLaunch = (string)data["AppLaunch"];
            AppArgs = (string)data["AppArgs"];
        }
    }
}