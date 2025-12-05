using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    public class UploadProfileMeta
    {
        public string GUID;
        public string ProfileName;
        public string FilePath;

        public UploadProfileMeta()
        {
            
        }

        public static List<UploadProfileMeta> LoadFromProjectSettings()
        {
            List<UploadProfileMeta> profileMetas  = new List<UploadProfileMeta>();
            
            string[] files = Directory.GetFiles(WindowUploadTab.UploadProfilePath, "*.json");
            if (files.Length > 0)
            {
                for (int j = 0; j < files.Length; j++)
                {
                    string file = files[j];
                    string json = File.ReadAllText(file);
                    UploadProfileSavedData savedData;
                    try
                    {
                        savedData = JSON.DeserializeObject<UploadProfileSavedData>(json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to deserialize UploadProfileSavedData from file: {file}. Skipping this file.");
                        Debug.LogException(e);
                        continue;
                    }

                    if (savedData == null)
                    {
                        Debug.LogWarning($"Failed to deserialize UploadProfileSavedData from file: {file}. Skipping this file.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(savedData.GUID))
                    {
                        savedData.GUID = Guid.NewGuid().ToString().Substring(0, 6);
                    }

                    UploadProfileMeta metaData = new UploadProfileMeta();
                    metaData.GUID = savedData.GUID;
                    metaData.ProfileName = savedData.ProfileName;
                    metaData.FilePath = file;
                    profileMetas.Add(metaData);
                }

                if (profileMetas.Count > 0)
                {
                    profileMetas.Sort((a,b)=>
                    {
                        int compare = String.Compare(a.ProfileName, b.ProfileName, StringComparison.Ordinal);
                        if (compare == 0)
                        {
                            return String.Compare(a.GUID, b.GUID, StringComparison.Ordinal);
                        }
                        return compare;
                    });
                }
            }

            return profileMetas;
        }
    }
}