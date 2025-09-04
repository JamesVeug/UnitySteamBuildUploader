using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Wireframe
{
    public static class BuildUtils
    {
        public enum Architecture
        {
            /// <summary>
            ///   <para>Supported for MacOS, Windows and Linux.</para>
            /// </summary>
            x64,
            /// <summary>
            ///   <para>Supported for MacOS and Windows.</para>
            /// </summary>
            ARM64,
            /// <summary>
            ///   <para>Supported for MacOS.</para>
            /// </summary>
            x64ARM64,
            /// <summary>
            ///   <para>Supported for Windows.</para>
            /// </summary>
            x86,
        }
        
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
                        
                        if (subTarget == (int)StandaloneBuildSubtarget.Server)
                        {
                            os += " " + name;
                        }
                        return os;
                    }
                    return title.text;
                }
            }

            public string name;
            public string tooltip;
            public GUIContent title;
            public BuildTarget defaultTarget;
            public BuildTargetGroup targetGroup;
            public int subTarget;
            public List<BuildPlatform> derivedPlatforms = new List<BuildPlatform>();

            public override string ToString()
            {
                return $"{name} - {tooltip} - ({targetGroup} - {defaultTarget})";
            }
        }
        
        public static ManagedStrippingLevel CurrentStrippingLevel()
        {
#if UNITY_2021_1_OR_NEWER
            return PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(BuildTargetToPlatform()));
#else
            return PlayerSettings.GetManagedStrippingLevel(BuildTargetToPlatform());
#endif
        }

        public static ScriptingImplementation CurrentScriptingBackend()
        {
            // 0 - Mono
            // 1 - IL2CPP
#if UNITY_2021_1_OR_NEWER
            return PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(BuildTargetToPlatform()));
#else
            return PlayerSettings.GetScriptingBackend(BuildTargetToPlatform());
#endif
        }

        public static Dictionary<LogType, StackTraceLogType> CurrentStackTraceLogTypes()
        {
            Dictionary<LogType, StackTraceLogType> stackTraceLogTypes = new Dictionary<LogType, StackTraceLogType>();
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            {
                stackTraceLogTypes[logType] = PlayerSettings.GetStackTraceLogType(logType);
            }

            return stackTraceLogTypes;
        }

        public static Architecture CurrentTargetArchitecture()
        {
#if UNITY_2021_1_OR_NEWER
            BuildTarget value = BuildUtils.CurrentTargetPlatform();
            string buildTargetName = BuildPipeline.GetBuildTargetName(value);
            string lower = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, "Architecture").ToLower();
            if (Enum.TryParse(lower, out Architecture result))
            {
                return result;
            }
#endif
            
            // Assume always 64 bit
            return Architecture.x64;
        }

        public static BuildTarget CurrentTargetPlatform()
        {
            return EditorUserBuildSettings.activeBuildTarget;
        }

        public static BuildTargetGroup BuildTargetToPlatform()
        {
            return BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        }

        public static List<string> GetDefaultScriptingDefines()
        {
            List<string> defines = new List<string>();

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            string[] scriptingDefines = null;
#if UNITY_2021_1_OR_NEWER
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out scriptingDefines);
#else
            string value = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            scriptingDefines = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
#endif
            defines.AddRange(scriptingDefines);

            return defines;
        }

        public static List<string> GetDefaultScenes()
        {
            List<string> defaultScenes = new List<string>();
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    defaultScenes.Add(scene.path);
                }
            }

            return defaultScenes;
        }

        public static string GetDefaultProductName()
        {
            return Application.productName;
        }
    
        private static Type BuildPlatformsType = Type.GetType("UnityEditor.Build.BuildPlatforms, UnityEditor");
        private static PropertyInfo BuildPlatformsTypeInstanceProp = BuildPlatformsType.GetProperty("instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        private static List<BuildPlatform> BuildPlatforms = null;

        public static List<BuildPlatform> ValidPlatforms
        {
            get
            {
                if (BuildPlatforms == null)
                {
                    BuildPlatforms = GetValidPlatforms();
                }

                return BuildPlatforms;
            }
        }
        
        public static (BuildTargetGroup newTargetGroup, int newSubTarget, BuildTarget target) DrawPlatformPopup(BuildTargetGroup group, int subTarget, BuildTarget target)
        {
            List<BuildPlatform> platforms = GetValidPlatforms();
            int selectedIndex = Mathf.Max(0, platforms.FindIndex(a=>a.targetGroup == group && a.subTarget == subTarget && a.defaultTarget == target));
            
            // Draw list of platforms
            string[] names = new string[platforms.Count];
            for (int i = 0; i < platforms.Count; i++)
            {
                names[i] = platforms[i].DisplayName;
            }
            int newIndex = EditorGUILayout.Popup(selectedIndex, names);
            BuildTargetGroup newTargetGroup = platforms[newIndex].targetGroup;
            int newTargetGroupSubTarget = platforms[newIndex].subTarget;
            BuildTarget newTarget = platforms[newIndex].defaultTarget;
            
            return (newTargetGroup, newTargetGroupSubTarget, newTarget);
        }
        
        private static List<BuildPlatform> GetValidPlatforms()
        {
            object instance = BuildPlatformsTypeInstanceProp.GetValue(null);
            FieldInfo buildPlatformsField = BuildPlatformsType.GetField("buildPlatforms", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object result = buildPlatformsField.GetValue(instance);
            
            List<BuildPlatform> validPlatforms = new List<BuildPlatform>();
            foreach (var platform in (IList)result)
            {
                BuildPlatform buildPlatform = ToBuildPlatform(platform);
                if (buildPlatform != null)
                {
                    validPlatforms.Add(buildPlatform);

                    if (buildPlatform.targetGroup == BuildTargetGroup.Standalone)
                    {
                        buildPlatform.defaultTarget = BuildTarget.StandaloneWindows64;
                        
                        BuildPlatform macBuildPlatform = ToBuildPlatform(platform);
                        macBuildPlatform.defaultTarget = BuildTarget.StandaloneOSX;
                        validPlatforms.Add(macBuildPlatform);
                        
                        BuildPlatform linuxBuildPlatform = ToBuildPlatform(platform);
                        linuxBuildPlatform.defaultTarget = BuildTarget.StandaloneLinux64;
                        validPlatforms.Add(linuxBuildPlatform);
                    }
                }
            }
            
            return validPlatforms;

            BuildPlatform ToBuildPlatform(object platform)
            {
                var platformType = platform.GetType();
                var hideInUiField = platformType.GetField("hideInUi", BindingFlags.Instance | BindingFlags.Public);
                bool hideInUi = (bool)hideInUiField.GetValue(platform);
                if (hideInUi)
                {
                    return null;
                }

                var nameField = platformType.GetField("name", BindingFlags.Instance | BindingFlags.Public);
                var titleField = platformType.GetProperty("title", BindingFlags.Instance | BindingFlags.Public);
                var tooltipField = platformType.GetField("tooltip", BindingFlags.Instance | BindingFlags.Public);
                var defaultTargetField = platformType.GetField("defaultTarget", BindingFlags.Instance | BindingFlags.Public);
                var subtargetField = platformType.GetField("subtarget", BindingFlags.Instance | BindingFlags.Public);
                var targetGroupField = platformType.GetProperty("targetGroup", BindingFlags.Instance | BindingFlags.Public);
                var derivedPlatformsField = platformType.GetField("m_DerivedPlatforms", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                string name = (string)nameField.GetValue(platform);
                GUIContent title = (GUIContent)titleField.GetValue(platform);
                string tooltip = (string)tooltipField.GetValue(platform);
                int subTarget = subtargetField != null ? (int)subtargetField.GetValue(platform) : 0;
                BuildTarget defaultTargetValue = (BuildTarget)defaultTargetField.GetValue(platform);
                BuildTargetGroup targetGroupValue = (BuildTargetGroup)targetGroupField.GetValue(platform);
                // object internalDerivedPlatforms = derivedPlatformsField.GetValue(platform);
                // List<BuildPlatform> derivedPlatforms = new List<BuildPlatform>();
                // if (internalDerivedPlatforms != null)
                // {
                //     IEnumerable<BuildPlatform> derivedPlatformsArray = (IEnumerable<BuildPlatform>)internalDerivedPlatforms;
                //     foreach (var derivedPlatform in derivedPlatformsArray)
                //     {
                //         BuildPlatform dp = ToBuildPlatform(derivedPlatform);
                //         derivedPlatforms.Add(dp);
                //     }
                // }
                
                
                MethodInfo IsBuildPlatformSupportedMethod = typeof(BuildPipeline).GetMethod("IsBuildPlatformSupported", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                bool isSupported = (bool)IsBuildPlatformSupportedMethod.Invoke(null, new object[] { defaultTargetValue });
                if (!isSupported)
                {
                    // return null;
                }

                BuildPlatform buildPlatform = new BuildPlatform
                {
                    name = name,
                    tooltip = tooltip,
                    title = title,
                    defaultTarget = defaultTargetValue,
                    targetGroup = targetGroupValue,
                    subTarget = subTarget
                };
                return buildPlatform;
            }
        }

        public static bool TrySwitchPlatform(BuildTargetGroup TargetPlatform, int TargetPlatformSubTarget, BuildTarget Target, Architecture architecture, UploadTaskReport.StepResult stepResult)
        {
            if ((int)TargetPlatform == 0)
            {
                stepResult?.AddError("No target platform selected");
                stepResult?.SetFailed("No target platform selected");
                return false;
            }

            if (TargetPlatform == BuildTargetGroup.Standalone && TargetPlatformSubTarget == 0)
            {
                stepResult?.AddError("No target platform sub-target selected for Standalone platform");
                stepResult?.SetFailed("No target platform sub-target selected for Standalone platform");
                return false;
            }
            
            if ((int)Target == 0)
            {
                stepResult?.AddError("No target selected");
                stepResult?.SetFailed("No target selected");
                return false;
            }
            
            if (!IsEditorSetToTarget(TargetPlatform, TargetPlatformSubTarget, Target))
            {
                stepResult?.AddLog($"Switching build target to {TargetPlatform} subTarget {TargetPlatformSubTarget} target {Target}");
                bool switched = SwitchToBuildTarget(TargetPlatform, TargetPlatformSubTarget, Target, architecture);
                if (!switched)
                {
                    stepResult?.AddError($"Failed to switch build target to {Target}");
                    stepResult?.SetFailed("Failed to switch build target. Please check the console for more details.");
                    return false;
                }

                if (EditorUserBuildSettings.activeBuildTarget != Target)
                {
                    stepResult?.AddError($"Failed to switch build target to {Target}. Current target is {EditorUserBuildSettings.activeBuildTarget}");
                    stepResult?.SetFailed("Failed to switch build target. Please check the console for more details.");
                    return false;
                }

                stepResult?.AddLog($"Switched build target to {TargetPlatform}");
                return true;
            }

            stepResult?.AddLog($"Build target is already set to {TargetPlatform} - {Target} - {TargetPlatformSubTarget}");
            return true;
        }

        public static Architecture DrawArchitecturePopup(BuildTargetGroup targetPlatform, BuildTarget target, Architecture architecture)
        {
            if (targetPlatform != BuildTargetGroup.Standalone)
            {
                return architecture;
            }
            
            List<Architecture> options = new List<Architecture>();
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    options.Add(Architecture.x86);
                    options.Add(Architecture.x64);
                    options.Add(Architecture.ARM64);
                    break;
                case BuildTarget.StandaloneOSX:
                    options.Add(Architecture.x64);
                    options.Add(Architecture.ARM64);
                    options.Add(Architecture.x64ARM64);
                    break;
                case BuildTarget.StandaloneLinux64:
                    options.Add(Architecture.x64);
                    break;
            }

            if (options.Count > 0)
            {
                string[] names = new string[options.Count];
                for (int i = 0; i < options.Count; i++)
                {
                    names[i] = options[i].ToString();
                }

                int selectedIndex = Mathf.Max(0, options.IndexOf(architecture));
                int newIndex = EditorGUILayout.Popup(selectedIndex, names);
                return options[newIndex];
            }
            
            return architecture;
        }

        public static int CurrentSubTarget()
        {
            MethodInfo methodInfo = typeof(EditorUserBuildSettings).GetMethod("GetSelectedSubtargetFor", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            int subTarget = (int)methodInfo.Invoke(null, new object[] { EditorUserBuildSettings.activeBuildTarget });
            return subTarget;
        }
        
        public static bool SwitchToBuildTarget(BuildTargetGroup targetPlatform, int subTarget, BuildTarget target, Architecture architecture)
        {
            if (IsEditorSetToTarget(targetPlatform, subTarget, target))
            {
                return true;
            }

            try
            {
                switch (targetPlatform)
                {
                    case BuildTargetGroup.Standalone:
                        EditorUserBuildSettings.standaloneBuildSubtarget = (StandaloneBuildSubtarget)subTarget;
                        break;
                    case BuildTargetGroup.PS4:
                        EditorUserBuildSettings.ps4BuildSubtarget = (PS4BuildSubtarget)subTarget;
                        break;
                }

                EditorUserBuildSettings.standaloneBuildSubtarget = (StandaloneBuildSubtarget)subTarget;
                MethodInfo methodInfo = typeof(EditorUserBuildSettings).GetMethod("SwitchActiveBuildTargetAndSubtarget",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                methodInfo.Invoke(null, new object[] { target, subTarget });
                
#if UNITY_2021_1_OR_NEWER
                PlayerSettings.SetArchitecture(NamedBuildTarget.FromBuildTargetGroup(targetPlatform), (int)architecture);
#else
                PlayerSettings.SetArchitecture(targetPlatform, (int)architecture);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to switch build target to {targetPlatform} - {target} - {subTarget}: {e}");
                return false;
            }

            return true;
        }

        public static bool IsEditorSetToTarget(BuildTargetGroup targetPlatform, int targetPlatformSubTarget, BuildTarget target)
        {
            if(EditorUserBuildSettings.selectedBuildTargetGroup != targetPlatform)
            {
                return false;
            }
            
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                return false;
            } 
            
            int currentSubTarget = CurrentSubTarget();
            if (currentSubTarget != targetPlatformSubTarget)
            {
                return false;
            }

            return true;
        }

        public static List<BuildTarget> ValidTargetsForPlatform(BuildTargetGroup buildTargetGroup)
        {
            List<BuildTarget> targets = new List<BuildTarget>();
            foreach (var platform in ValidPlatforms)
            {
                if (platform.targetGroup != buildTargetGroup)
                {
                    continue;
                }
                
                if (!targets.Contains(platform.defaultTarget)) 
                    targets.Add(platform.defaultTarget);
            }

            return targets;
        }
    }
}