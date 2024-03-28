using System;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Manifest containing all the information about the build make on UnityCloud
    /// UnityCloud makes one of these for every build
    /// </summary>
    [Serializable]
    public class CloudBuildManifest
    {
        public static CloudBuildManifest Instance
        {
            get
            {
                if (m_instance == null)
                {
                    // Get Manifest created by Unity Cloud
                    var manifest =
                        Resources.Load<TextAsset>(
                            "UnityCloudBuildManifest"); // UnityCloud puts this in Resources when it builds
                    if (manifest != null)
                    {
                        Debug.Log("[CloudUtil] Found Manifest");
                        m_instance = JsonUtility.FromJson<CloudBuildManifest>(manifest.text);
                        Debug.Log("[CloudUtil] Build: " + m_instance.buildNumber);
                    }
                    else
                    {
                        Debug.LogError("[CloudUtil] Could not find Manifest");
                        m_instance = new CloudBuildManifest();
                        m_instance.Save();
                    }
                }

                return m_instance;
            }
        }

        private static CloudBuildManifest m_instance;

        // Commit or changelist built by UCB
        public string CommitID
        {
            get => scmCommitId;
            set => scmCommitId = value;
        }

        // Name of the branch that was built
        public string Branch
        {
            get => scmBranch;
            set => scmBranch = value;
        }

        // The Unity Cloud Build number corresponding to this build
        public int BuildNumber
        {
            get => buildNumber;
            set => buildNumber = value;
        }

        // The UTC timestamp when the build process was started
        public string StartTime
        {
            get => buildStartTime;
            set => buildStartTime = value;
        }

        // The UCB project identifier
        public string ProjectId
        {
            get => projectId;
            set => projectId = value;
        }

        // (iOS and Android only) The bundleIdentifier configured in Unity Cloud Build
        public string BundleId
        {
            get => bundleId;
            set => bundleId = value;
        }

        // The version of Unity used by UCB to create the build
        public string UnityVersion
        {
            get => unityVersion;
            set => unityVersion = value;
        }

        // (iOS only) The version of XCode used to build the project
        public string XCodeVersion
        {
            get => xcodeVersion;
            set => xcodeVersion = value;
        }

        // The name of the project build target that was built. Currently, this will correspond to the platform, as either "default-web”, “default-ios”, or “default-android"
        public string CloudBuildTargetName
        {
            get => cloudBuildTargetName;
            set => cloudBuildTargetName = value;
        }

        [SerializeField] private string scmCommitId;
        [SerializeField] private string scmBranch;
        [SerializeField] private int buildNumber;
        [SerializeField] private string buildStartTime;
        [SerializeField] private string projectId;
        [SerializeField] private string bundleId;
        [SerializeField] private string unityVersion;
        [SerializeField] private string xcodeVersion;
        [SerializeField] private string cloudBuildTargetName;

        public void Save()
        {
            string json = JsonUtility.ToJson(m_instance);
            File.WriteAllText($"{Application.dataPath}/Resources/UnityCloudBuildManifest.json",
                json); // UnityCloud puts this in Resources when it builds
        }
    }
}