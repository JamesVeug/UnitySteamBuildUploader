using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfig
    {
        public enum Architecture
        {
            Unknown,
            x86_64,
            x86_32,
        }
        
        public string GUID;
        public string BuildName;
        public string ProductName;
        public List<string> Scenes;
        public List<string> ExtraScriptingDefines;
        
        // Options
        public bool IsDevelopmentBuild;
        public bool BuildScriptsOnly;
        public bool AllowDebugging;
        public bool ConnectProfiler;
        public bool EnableDeepProfilingSupport;
        
        // Platform specific settings
        public bool SwitchTargetPlatform = false;
        public BuildTargetGroup TargetPlatform;
        public Architecture TargetArchitecture;
        public Dictionary<LogType, StackTraceLogType> StackTraceLogTypes;
        public ManagedStrippingLevel StrippingLevel = ManagedStrippingLevel.Disabled;
        // public ScriptingImplementation ScriptingBackend; // TODO: IL2CPP settings also

        public void SetupDefaults()
        {
            BuildName = "New Build";
            GUID = Guid.NewGuid().ToString().Substring(0, 6);
            Scenes = GetDefaultScenes();
            ProductName = GetDefaultProductName();
            ExtraScriptingDefines = GetDefaultScriptingDefines();
            TargetPlatform = BuildTargetToPlatform();
            TargetArchitecture = CurrentTargetArchitecture();
            StackTraceLogTypes = CurrentStackTraceLogTypes();
            // ScriptingBackend = CurrentScriptingBackend(); // TODO:
            StrippingLevel = CurrentStrippingLevel();
        }

        private ManagedStrippingLevel CurrentStrippingLevel()
        {
#if UNITY_2021_1_OR_NEWER
            return PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform));
#else
            return PlayerSettings.GetManagedStrippingLevel(BuildTargetToPlatform());
#endif
        }

        private ScriptingImplementation CurrentScriptingBackend()
        {
            // 0 - Mono
            // 1 - IL2CPP
#if UNITY_2021_1_OR_NEWER
            return PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform));
#else
            return PlayerSettings.GetScriptingBackend(BuildTargetToPlatform());
#endif
        }

        private Dictionary<LogType, StackTraceLogType> CurrentStackTraceLogTypes()
        {
            Dictionary<LogType, StackTraceLogType> stackTraceLogTypes = new Dictionary<LogType, StackTraceLogType>();
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            {
                stackTraceLogTypes[logType] = PlayerSettings.GetStackTraceLogType(logType);
            }
            return stackTraceLogTypes;
        }

        private Architecture CurrentTargetArchitecture()
        {
            // 0 - None
            // 1 - ARM64
            // 2 - Universal (I'm assuming this is 32 bit)
#if UNITY_2021_1_OR_NEWER
            int architecture = PlayerSettings.GetArchitecture(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform));
#else
            int architecture = PlayerSettings.GetArchitecture(BuildTargetToPlatform());
#endif
            switch (architecture)
            {
                case 0:
                    return Architecture.Unknown;
                case 1:
                    return Architecture.x86_64;
                case 2:
                    return Architecture.x86_32;
                default:
                    return Architecture.Unknown;
            }
        }

        public static BuildTarget CurrentTargetPlatform()
        {
            return EditorUserBuildSettings.activeBuildTarget;
        }
        
        public static BuildTargetGroup BuildTargetToPlatform()
        {
            return BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        }
        
        public List<string> GetDefaultScriptingDefines()
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

        private List<string> GetDefaultScenes()
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

        private string GetDefaultProductName()
        {
            return Application.productName;
        }

        public Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "GUID", GUID },
                { "BuildName", BuildName },
                { "ProductName", ProductName },
                { "ExtraScriptingDefines", ExtraScriptingDefines ?? new List<string>() },
                { "Scenes", Scenes ?? new List<string>() },
                { "IsDevelopmentBuild", IsDevelopmentBuild },
                { "BuildScriptsOnly", BuildScriptsOnly },
                { "AllowDebugging", AllowDebugging },
                { "ConnectProfiler", ConnectProfiler },
                { "EnableDeepProfilingSupport", EnableDeepProfilingSupport },
                { "TargetPlatform", TargetPlatform.ToString() },
                { "TargetArchitecture", TargetArchitecture.ToString() },
                { "StackTraceLogTypes", StackTraceLogTypes.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString()) },
                { "StrippingLevel", StrippingLevel.ToString() }
            };
            return dict;
        }
        
        public void Deserialize(Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("GUID", out var guidData) && guidData is string guid)
            {
                GUID = guid;
            }
            else
            {
                GUID = Guid.NewGuid().ToString().Substring(0, 6);
            }
            
            if (dict.TryGetValue("BuildName", out var buildNameData) && buildNameData is string buildName)
            {
                BuildName = buildName;
            }
            else
            {
                BuildName = "New Build";
            }
            
            if (dict.TryGetValue("ProductName", out var productNameData) && productNameData is string productName)
            {
                ProductName = productName;
            }
            else
            {
                ProductName = GetDefaultProductName();
            }
            
            if (dict.TryGetValue("ExtraScriptingDefines", out var extraDefinesData) && extraDefinesData != null)
            {
                ExtraScriptingDefines = ((List<object>)extraDefinesData).Cast<string>().ToList();
            }
            else
            {
                ExtraScriptingDefines = GetDefaultScriptingDefines();
            }
            
            if (dict.TryGetValue("Scenes", out var scenesData) && scenesData != null)
            {
                Scenes = ((List<object>)scenesData).Cast<string>().ToList();
            }
            else
            {
                Scenes = GetDefaultScenes();
            }
            
            if (dict.TryGetValue("IsDevelopmentBuild", out var isDevBuildData) && isDevBuildData is bool isDevBuild)
            {
                IsDevelopmentBuild = isDevBuild;
            }
            else
            {
                IsDevelopmentBuild = false;
            }
            
            if (dict.TryGetValue("BuildScriptsOnly", out var buildScriptsOnlyData) && buildScriptsOnlyData is bool buildScriptsOnly)
            {
                BuildScriptsOnly = buildScriptsOnly;
            }
            else
            {
                BuildScriptsOnly = false;
            }
            
            if (dict.TryGetValue("AllowDebugging", out var allowDebuggingData) && allowDebuggingData is bool allowDebugging)
            {
                AllowDebugging = allowDebugging;
            }
            else
            {
                AllowDebugging = false;
            }
            
            if (dict.TryGetValue("ConnectProfiler", out var connectProfilerData) && connectProfilerData is bool connectProfiler)
            {
                ConnectProfiler = connectProfiler;
            }
            else
            {
                ConnectProfiler = false;
            }
            
            if (dict.TryGetValue("EnableDeepProfilingSupport", out var enableDeepProfilingData) && enableDeepProfilingData is bool enableDeepProfiling)
            {
                EnableDeepProfilingSupport = enableDeepProfiling;
            }
            else
            {
                EnableDeepProfilingSupport = false;
            }
            
            if (dict.TryGetValue("TargetPlatform", out var targetPlatformData) && targetPlatformData is string targetPlatformStr)
            {
                if (Enum.TryParse(targetPlatformStr, out BuildTargetGroup targetPlatform))
                {
                    TargetPlatform = targetPlatform;
                }
                else
                {
                    Debug.LogWarning($"Invalid TargetPlatform value: {targetPlatformStr}. Defaulting to current platform.");
                    TargetPlatform = BuildTargetToPlatform();
                }
            }
            else
            {
                TargetPlatform = BuildTargetToPlatform();
            }
            
            if (dict.TryGetValue("TargetArchitecture", out var targetArchitectureData) && targetArchitectureData is string targetArchitectureStr)
            {
                if (Enum.TryParse(targetArchitectureStr, out Architecture targetArchitecture))
                {
                    TargetArchitecture = targetArchitecture;
                }
                else
                {
                    Debug.LogWarning($"Invalid TargetArchitecture value: {targetArchitectureStr}. Defaulting to Unknown.");
                    TargetArchitecture = Architecture.Unknown;
                }
            }
            else
            {
                TargetArchitecture = CurrentTargetArchitecture();
            }
            
            if (dict.TryGetValue("StackTraceLogTypes", out var stackTraceLogTypesData) && stackTraceLogTypesData is Dictionary<string, string> stackTraceLogTypesDict)
            {
                StackTraceLogTypes = stackTraceLogTypesDict.ToDictionary(
                    kvp => (LogType)Enum.Parse(typeof(LogType), kvp.Key),
                    kvp => (StackTraceLogType)Enum.Parse(typeof(StackTraceLogType), kvp.Value));
            }
            else
            {
                StackTraceLogTypes = CurrentStackTraceLogTypes();
            }
            
            if (dict.TryGetValue("StrippingLevel", out var strippingLevelData) && strippingLevelData is string strippingLevelStr)
            {
                if (Enum.TryParse(strippingLevelStr, out ManagedStrippingLevel strippingLevel))
                {
                    StrippingLevel = strippingLevel;
                }
                else
                {
                    Debug.LogWarning($"Invalid StrippingLevel value: {strippingLevelStr}. Defaulting to Disabled.");
                    StrippingLevel = ManagedStrippingLevel.Disabled;
                }
            }
            else
            {
                StrippingLevel = CurrentStrippingLevel();
            }
        }

        public BuildOptions GetBuildOptions()
        {
            BuildOptions buildOptions = BuildOptions.None;
            
            if (IsDevelopmentBuild)
                buildOptions |= BuildOptions.Development;

            if (AllowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;

            if (BuildScriptsOnly)
                buildOptions |= BuildOptions.BuildScriptsOnly;

            if (ConnectProfiler)
                buildOptions |= BuildOptions.ConnectWithProfiler;
            
            if (EnableDeepProfilingSupport)
                buildOptions |= BuildOptions.EnableDeepProfilingSupport;

            return buildOptions;
        }

        public bool ApplySettings(StringFormatter.Context context, UploadTaskReport.StepResult stepResult = null)
        {
            // Switch to the build target if necessary
            if (SwitchTargetPlatform)
            {
                BuildTarget buildTarget = CalculateTarget();
                if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
                {
                    stepResult?.AddLog($"Switching build target to {buildTarget}");
                    bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(TargetPlatform, buildTarget);
                    if (!switched)
                    {
                        stepResult?.AddError($"Failed to switch build target to {buildTarget}");
                        stepResult?.SetFailed(
                            "Failed to switch build target. Please check the console for more details.");
                        return false;
                    }
                    else if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
                    {
                        stepResult?.AddError(
                            $"Failed to switch build target to {buildTarget}. Current target is {EditorUserBuildSettings.activeBuildTarget}");
                        stepResult?.SetFailed(
                            "Failed to switch build target. Please check the console for more details.");
                        return false;
                    }

                    stepResult?.AddLog($"Switched build target to {TargetPlatform}");
                }
            }
            else
            {
                stepResult?.AddLog($"Override Target Platform is disabled so using current platform {EditorUserBuildSettings.activeBuildTarget}");
            }

            PlayerSettings.productName = StringFormatter.FormatString(ProductName, context);
            string[] defines = ExtraScriptingDefines.Select(a=>StringFormatter.FormatString(a, context)).ToArray();
#if UNITY_2021_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform), defines);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform), StrippingLevel);
            PlayerSettings.SetArchitecture(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform), (int)TargetArchitecture);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(TargetPlatform, string.Join(";", defines));
            PlayerSettings.SetManagedStrippingLevel(TargetPlatform, StrippingLevel);
            PlayerSettings.SetArchitecture(TargetPlatform, (int)TargetArchitecture);
#endif
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogTypes[LogType.Error]);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogTypes[LogType.Assert]);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogTypes[LogType.Warning]);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogTypes[LogType.Log]);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogTypes[LogType.Exception]);
            // PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(TargetPlatform), CurrentScriptingBackend());
            
            EditorUserBuildSettings.development = IsDevelopmentBuild;
            EditorUserBuildSettings.connectProfiler = ConnectProfiler;
            EditorUserBuildSettings.allowDebugging = AllowDebugging;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = EnableDeepProfilingSupport;
            
            
            // Scene list
            if (Scenes == null || Scenes.Count == 0)
            {
                EditorBuildSettings.scenes = Array.Empty<EditorBuildSettingsScene>();
            }
            else
            {
                EditorBuildSettings.scenes = Scenes.Select(scene => new EditorBuildSettingsScene(scene, true)).ToArray();
            }

            return true;
        }

        public BuildTarget CalculateTarget()
        {
            BuildTarget currentTarget = BuildTarget.NoTarget;
            switch (TargetPlatform)
            {
                case BuildTargetGroup.Standalone:
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    if (TargetArchitecture == Architecture.x86_64)
                    {
                        // 64 bit
                        currentTarget = BuildTarget.StandaloneWindows64;
                    }
                    else
                    {
                        // 32 bit
                        currentTarget = BuildTarget.StandaloneWindows; // Default to Windows for Standalone
                    }
#elif UNITY_EDITOR_OSX
                    // Use SetArchitecture to define 32bit / 64bit
                    currentTarget = BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                    if (TargetArchitecture == Architecture.x86_64)
                    {
                        // 64 bit
                        currentTarget = BuildTarget.StandaloneLinux64;
                    }
                    else
                    {
                        // 32 bit
                        throw new NotSupportedException("32-bit Linux builds are not supported. Please use StandaloneLinux64.");
                    }
#else
                    throw new NotSupportedException("Unsupported standalone platform. Please specify a valid architecture for Standalone builds.");
#endif
                    break;
                case BuildTargetGroup.WebGL:
                    currentTarget = BuildTarget.WebGL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return currentTarget;
        }

        public string GetFormattedProductName(StringFormatter.Context ctx)
        {
            string formatted = StringFormatter.FormatString(ProductName, ctx);
            if (TargetPlatform == BuildTargetGroup.Standalone)
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return $"{formatted}.exe"; // For Windows, the executable is a .exe file
#elif UNITY_MAC
                return $"{formatted}.app"; // For macOS, the executable is a .app bundle
#else
                return $"{formatted}"; // Default for other platforms
#endif
            }

            return formatted;
        }
    }
}