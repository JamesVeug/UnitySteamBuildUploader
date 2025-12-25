using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public class UploadProfileSavedData
    {
        public const int CurrentVersion = 2; // Current version of the saved data

        // Used to migrate data between different versions of the BuildUploader package
        public int Version;
        
        // Unique identifier for the profile, used to differentiate between with the same name
        public string GUID;
        
        // The name of the profile, used to identify it in the UI
        public string ProfileName;
        
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> Actions = new List<Dictionary<string, object>>();

        public UploadProfile ToUploadProfile()
        {
            UploadProfile loadedProfile = new UploadProfile();
            loadedProfile.ProfileName = ProfileName;
            loadedProfile.GUID = GUID;
            for (int i = 0; i < Data.Count; i++)
            {
                try
                {
                    UploadConfig uploadConfig = new UploadConfig();
                    Dictionary<string, object> configData = Data[i];
                    uploadConfig.Deserialize(configData);
                    loadedProfile.UploadConfigs.Add(uploadConfig);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load build config: #" + (i + 1));
                    Debug.LogException(e);
                    UploadConfig uploadConfig = new UploadConfig();
                    loadedProfile.UploadConfigs.Add(uploadConfig);
                }
            }

            for (int i = 0; i < Actions.Count; i++)
            {
                try
                {
                    UploadConfig.UploadActionData actionData = new UploadConfig.UploadActionData();
                    actionData.Deserialize(Actions[i]);
                    loadedProfile.Actions.Add(actionData);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load pre upload action: #" + (i + 1));
                    Debug.LogException(e);
                }
            }
            
            return loadedProfile;
        }
        
        public static UploadProfileSavedData FromUploadProfile(UploadProfile profile)
        {
            UploadProfileSavedData data = new UploadProfileSavedData();
            data.Version = CurrentVersion;
            data.GUID = profile.GUID;
            data.ProfileName = profile.ProfileName;
            for (int i = 0; i < profile.UploadConfigs.Count; i++)
            {
                data.Data.Add(profile.UploadConfigs[i].Serialize());
            }
            for (int i = 0; i < profile.Actions.Count; i++)
            {
                data.Actions.Add(profile.Actions[i].Serialize());
            }

            return data;
        }

        public static UploadProfileSavedData FromJSON(string json)
        {
            OnlyVersion jsonVersion = JSON.DeserializeObject<OnlyVersion>(json);
            if (jsonVersion.Version == CurrentVersion)
            {
                return JSON.DeserializeObject<UploadProfileSavedData>(json);
            }
            
            // Migrate the data to the current version
            UploadProfileSavedData data = new UploadProfileSavedData();
            data.Version = CurrentVersion;
            switch (jsonVersion.Version)
            {
                case 1:
                    UploadProfileSavedData_V1 v1 = JSON.DeserializeObject<UploadProfileSavedData_V1>(json);
                    data.GUID = v1.GUID;
                    data.ProfileName = v1.ProfileName;
                    data.Data = v1.Data;
                    data.Actions = v1.PostUploads;
                    break;
                default:
                    throw new ArgumentException("Unsupported version: " + jsonVersion.Version);
            }
            
            return data;
        }
    }
    
    [Serializable, Obsolete("Deprecated in v3.0.0. Use UploadProfileData instead")]
    internal class UploadTabData
    {
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();
    }
    
    [Serializable]
    internal class OnlyVersion
    {
        public int Version;
    }
}