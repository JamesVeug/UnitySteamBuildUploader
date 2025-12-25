using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Renamed PostUploads to Actions in v3.2.0
    /// </summary>
    [Serializable]
    public class UploadProfileSavedData_V1
    {
        // Used to migrate data between different versions of the BuildUploader package
        public int Version;
        
        // Unique identifier for the profile, used to differentiate between with the same name
        public string GUID;
        
        // The name of the profile, used to identify it in the UI
        public string ProfileName;
        
        [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();
    }
}