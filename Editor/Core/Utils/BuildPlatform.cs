using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class BuildPlatform
    {
        public string DisplayName
        {
            get
            {
                if (targetGroup == BuildTargetGroup.Standalone)
                {
                    string os = "";
                    switch (defaultTarget)
                    {
                        case BuildTarget.StandaloneWindows:
                        case BuildTarget.StandaloneWindows64:
                            os = "Windows";
                            break;
                        case BuildTarget.StandaloneOSX:
                            os = "macOS";
                            break;
                        case BuildTarget.StandaloneLinux64:
                            os = "Linux";
                            break;
                    }
                        
#if UNITY_2021_1_OR_NEWER
                    if (subTarget == (int)StandaloneBuildSubtarget.Server)
                    {
                        os += " " + name;
                    }
#endif
                    return os;
                }
                return title.text;
            }
        }

        public string name;
        public string tooltip;
        public bool installed;
        public bool supported;
        public GUIContent title;
        public BuildTarget defaultTarget;
        public BuildTargetGroup targetGroup;
        public int subTarget;
        public List<BuildPlatform> derivedPlatforms = new List<BuildPlatform>();

        public override string ToString()
        {
            return $"{name} - {tooltip} - ({targetGroup} - {defaultTarget})";
        }
        
        public static BuildPlatform ToBuildPlatform(object platform)
        {
            Type platformType = platform.GetType();
#if UNITY_6000_0_OR_NEWER
            FieldInfo hideInUiField = platformType.GetField("hideInUi", BindingFlags.Instance | BindingFlags.Public);
            bool hideInUi = (bool)hideInUiField.GetValue(platform);
            if (hideInUi)
            {
                return null;
            }
#endif

            var nameField = platformType.GetField("name", BindingFlags.Instance | BindingFlags.Public);
            var titleField = platformType.GetProperty("title", BindingFlags.Instance | BindingFlags.Public);
            var tooltipField = platformType.GetField("tooltip", BindingFlags.Instance | BindingFlags.Public);
            var defaultTargetField = platformType.GetField("defaultTarget", BindingFlags.Instance | BindingFlags.Public);
            var subtargetField = platformType.GetField("subtarget", BindingFlags.Instance | BindingFlags.Public);
            
            string name = (string)nameField.GetValue(platform);
            GUIContent title = (GUIContent)titleField.GetValue(platform);
            string tooltip = (string)tooltipField.GetValue(platform);
            int subTarget = subtargetField != null ? (int)subtargetField.GetValue(platform) : 0;
            
            BuildTarget defaultTargetValue = (BuildTarget)defaultTargetField.GetValue(platform);
            
#if UNITY_6000_0_OR_NEWER // Build Profiles were added came with a massive refactor
            FieldInfo installedField = platformType.GetField("installed", BindingFlags.Instance | BindingFlags.Public);
            bool installed = (bool)installedField?.GetValue(platform);
            
            PropertyInfo targetGroupField = platformType.GetProperty("targetGroup", BindingFlags.Instance | BindingFlags.Public);
            BuildTargetGroup targetGroupValue = (BuildTargetGroup)targetGroupField.GetValue(platform);

            MethodInfo IsBuildPlatformSupportedMethod = typeof(BuildPipeline).GetMethod("IsBuildPlatformSupported", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            bool isSupported = (bool)IsBuildPlatformSupportedMethod.Invoke(null, new object[] { defaultTargetValue });
#elif UNITY_2022_2_OR_NEWER
            
            PropertyInfo targetGroupField = platformType.GetProperty("targetGroup", BindingFlags.Instance | BindingFlags.Public);
            BuildTargetGroup targetGroupValue = (BuildTargetGroup)targetGroupField.GetValue(platform);
            bool isSupported = BuildPipeline.IsBuildTargetSupported(targetGroupValue, defaultTargetValue);
            
            // Copied from BuildPlayerWindow.cs
            bool installed = targetGroupValue == BuildTargetGroup.Standalone || BuildPipeline.GetPlaybackEngineDirectory(defaultTargetValue, BuildOptions.None, false) != string.Empty;
#else
            bool installed = true; // Don't support checking if its installed before uploading
            
            FieldInfo targetGroupField = platformType.GetField("targetGroup", BindingFlags.Instance | BindingFlags.Public);
            BuildTargetGroup targetGroupValue = (BuildTargetGroup)targetGroupField.GetValue(platform);
            
            bool isSupported = BuildPipeline.IsBuildTargetSupported(targetGroupValue, defaultTargetValue);
#endif
            
            BuildPlatform buildPlatform = new BuildPlatform
            {
                name = name,
                tooltip = tooltip,
                title = title,
                installed = installed,
                supported = isSupported,
                defaultTarget = defaultTargetValue,
                targetGroup = targetGroupValue,
                subTarget = subTarget
            };
            return buildPlatform;
        }
    }
}