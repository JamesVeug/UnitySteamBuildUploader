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
        
        // The name of the profile, used to identify it in the UI
        public string ProfileName;
        
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();
    }
    
    [Serializable, Obsolete("Deprecated in v3.0.0. Use UploadProfileData instead")]
    internal class UploadTabData
    {
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();
    }
}