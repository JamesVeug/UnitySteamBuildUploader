using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class EpicGamesUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/EpicGamesConfig.json";

        private static EpicGamesAppData data;

        public static EpicGamesAppData GetEpicGamesData(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("EpicGamesConfig does not exist. Creating new file");
                    data = new EpicGamesAppData();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<EpicGamesAppData>(json);
            if (data == null)
            {
                Debug.Log("EpicGamesConfig has bad json so creating new config");
                data = new EpicGamesAppData();
                data.Initialize();
                Save();
            }
        }

        public static void Save()
        {
            if (data != null)
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                if (!File.Exists(FilePath))
                {
                    var stream = File.Create(FilePath);
                    stream.Close();
                }

                File.WriteAllText(FilePath, json);
            }
        }

        public static void RefreshAllPopups()
        {
            OrganizationPopup.Refresh();
            ProductPopup.Refresh();
            AllGamesPopup.Refresh();
            ArtifactPopup.Refresh();
        }

        public static EpicGamesOrganizationPopup OrganizationPopup => m_organizationPopup ?? (m_organizationPopup = new EpicGamesOrganizationPopup());
        private static EpicGamesOrganizationPopup m_organizationPopup;
        
        public static EpicGamesAllGamesPopup AllGamesPopup => m_artifactAllGamesPopup ?? (m_artifactAllGamesPopup = new EpicGamesAllGamesPopup());
        private static EpicGamesAllGamesPopup m_artifactAllGamesPopup;
        
        public static EpicGamesGamesPopup ProductPopup => m_artifactGamesPopup ?? (m_artifactGamesPopup = new EpicGamesGamesPopup());
        private static EpicGamesGamesPopup m_artifactGamesPopup;
        
        public static EpicGamesArtifactPopup ArtifactPopup => m_artifactPopup ?? (m_artifactPopup = new EpicGamesArtifactPopup());
        private static EpicGamesArtifactPopup m_artifactPopup;
    }

    internal class EpicGamesAppData
    {
        public List<EpicGamesOrganization> Organizations = new List<EpicGamesOrganization>();
        
        public void Initialize()
        {
            Organizations = new List<EpicGamesOrganization>(2);
        }
    }

    [Serializable]
    internal class EpicGamesOrganization : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string OrganizationID;
        public string Name;
        public List<EpicGamesProduct> Products;
        
        public EpicGamesOrganization()
        {
            ID = 0;
            Name = "My Organization";
            OrganizationID = "myorganization-xxxxxxxx";
            Products = new List<EpicGamesProduct>();
        }
        
        public EpicGamesOrganization(int id, string name)
        {
            ID = id;
            Name = name;
            OrganizationID = "myorganization-xxxxxxxx";
            Products = new List<EpicGamesProduct>();
        }
    }

    [Serializable]
    public class EpicGamesProduct : DropdownElement
    {
        public enum SecretTypes
        {
            EnvVar,
            ClientSecret
        }
        
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        
        public string ProductID;
        public string ClientID;
        public SecretTypes SecretType;
        public string ClientSecretEnvVar;
        public string Name;
        
        public List<EpicGamesArtifact> Artifacts = new List<EpicGamesArtifact>();
        
        public EpicGamesProduct()
        {
            ID = 0;
            Name = "Template";
            ProductID = "mygame-00000000";
            ClientID = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            ClientSecretEnvVar = "";
            SecretType = SecretTypes.EnvVar;
        }
        
        public EpicGamesProduct(int id, string name)
        {
            ID = id;
            Name = name;
            ProductID = "mygame-00000000";
            ClientID = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            ClientSecretEnvVar = "";
            SecretType = SecretTypes.EnvVar;
        }
    }

    [Serializable]
    public class EpicGamesArtifact : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string ArtifactID;
        public string Name;
        
        public EpicGamesArtifact()
        {
            ID = 0;
            ArtifactID = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            Name = "myartifact";
        }
        
        public EpicGamesArtifact(int id, string name, string artifactID)
        {
            ID = id;
            ArtifactID = artifactID;
            Name = name;
        }
    }
}