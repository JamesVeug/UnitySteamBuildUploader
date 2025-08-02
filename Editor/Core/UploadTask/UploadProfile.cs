﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    public class UploadProfile
    {
        // Unique ID to identify the build when multiple profiles with the same name exist
        public string GUID;
        
        // The name of the profile, used to identify it in the UI
        public string ProfileName;
        
        // 
        public List<UploadConfig> UploadConfigs = new List<UploadConfig>();
        public List<UploadConfig.PostUploadActionData> PostUploadActions = new List<UploadConfig.PostUploadActionData>();
        
        public static UploadProfile FromPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            string json = File.ReadAllText(fullPath);
            UploadProfileSavedData savedData = JSON.DeserializeObject<UploadProfileSavedData>(json);
            return savedData.ToUploadProfile();
        }
        
        /// <summary>
        /// Searches for an UploadProfile with the same GUID that exists in Project/BuildUploader/UploadProfiles/
        /// </summary>
        public static UploadProfile FromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            string[] files = Directory.GetFiles(WindowUploadTab.UploadProfilePath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                GUIDOnly minimal = JsonUtility.FromJson<GUIDOnly>(json);
                if (minimal.GUID == guid)
                {
                    return FromPath(file);
                }
            }
            
            return null;
        }

        [Serializable]
        private class GUIDOnly
        {
            public string GUID;
        }
        
        /// <summary>
        /// Searches for an UploadProfile with the same Name that exists in Project/BuildUploader/UploadProfiles/
        /// </summary>
        public static UploadProfile FromProfileName(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                return null;
            }

            string[] files = Directory.GetFiles(WindowUploadTab.UploadProfilePath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                NameOnly minimal = JsonUtility.FromJson<NameOnly>(json);
                if (minimal.ProfileName == profileName)
                {
                    return FromPath(file);
                }
            }
            
            return null;
        }

        [Serializable]
        private class NameOnly
        {
            public string ProfileName;
        }
    }
}