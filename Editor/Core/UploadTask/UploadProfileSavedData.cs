using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public class UploadProfileSavedData
    {
        public const int CurrentVersion = 1; // Current version of the saved data

        // Used to migrate data between different versions of the BuildUploader package
        public int Version;
        
        // Unique identifier for the profile, used to differentiate between with the same name
        public string GUID;
        
        // The name of the profile, used to identify it in the UI
        public string ProfileName;
        
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();

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
                    var jObject = Data[i];
                    uploadConfig.Deserialize(jObject);
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

            for (int i = 0; i < PostUploads.Count; i++)
            {
                try
                {
                    UploadConfig.PostUploadActionData actionData = new UploadConfig.PostUploadActionData();
                    actionData.Deserialize(PostUploads[i]);
                    loadedProfile.PostUploadActions.Add(actionData);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load post upload action: #" + (i + 1));
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
            for (int i = 0; i < profile.PostUploadActions.Count; i++)
            {
                data.PostUploads.Add(profile.PostUploadActions[i].Serialize());
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
}