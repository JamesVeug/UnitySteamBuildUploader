using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    /// <summary>
    /// Starts a new build using Unity's built in BuildProfiles.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(BuildProfileSource), "sources", "Chooses a BuildProfile to start a new build when uploading")]
    [UploadSource("BuildProfile", "Build Profile", false)]
    public partial class BuildProfileSource : ABuildSource<BuildProfileWrapper>
    {
        public BuildProfileSource()
        {
            // Required for reflection
        }
        
        public BuildProfileSource(BuildProfileWrapper BuildProfileWrapper, bool cleanBuild = false)
        {
            m_BuildConfig = BuildProfileWrapper;
            m_CleanBuild = cleanBuild;
        }
        
        public override BuildProfileWrapper GetBuildConfigToApply()
        {
            return m_BuildConfig; // TODO: Clone it to avoid people changing the profiles during a task
        }

        public override bool ApplyBuildConfig(BuildProfileWrapper config, UploadTaskReport.StepResult stepResult)
        {
            // ignore
            return true;
        }

        protected override BuildReport MakeBuild(BuildPlayerOptions options, UploadTaskReport.StepResult stepResult)
        {
            BuildPlayerWithProfileOptions profileOptions = new BuildPlayerWithProfileOptions();
            profileOptions.buildProfile = m_BuildConfig.Profile;
            profileOptions.locationPathName = options.locationPathName;
            profileOptions.options = options.options;
            BuildReport report = BuildPipeline.BuildPlayer(options);
            return report;
        }

        public override void SerializeBuildConfig(Dictionary<string, object> data)
        {
            data["BuildProfile"] = m_BuildConfig != null ? m_BuildConfig.GetGUID : "";
        }

        public override void DeserializeBuildConfig(Dictionary<string, object> data)
        {
            if (data.TryGetValue("BuildProfile", out var BuildProfileWrapperGuidObj) && BuildProfileWrapperGuidObj is string BuildProfileWrapperGuid)
            {
                m_BuildConfig = BuildProfileUIUtils.GetBuildProfiles().FirstOrDefault(a=>a.GetGUID == BuildProfileWrapperGuid);
                if (m_BuildConfig == null)
                {
                    Debug.LogWarning($"BuildProfileWrapper with GUID {BuildProfileWrapperGuid} not found.");
                }
            }
            else
            {
                Debug.LogWarning("BuildProfileWrapper GUID not found in serialized data.");
            }
        }

        public override bool CompareBuildConfig(IBuildSource other)
        {
            if(other is BuildProfileSource otherSource)
                return otherSource.m_BuildConfig == m_BuildConfig;
            
            return false;
        }
    }
}