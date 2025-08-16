using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Wireframe
{
    public partial class BuildConfig
    {
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

        public void SetupDefaults()
        {
            BuildName = "New Build";
            GUID = Guid.NewGuid().ToString().Substring(0, 6);
            Scenes = GetDefaultScenes();
            ProductName = GetDefaultProductName();
            ExtraScriptingDefines = GetDefaultScriptingDefines();
        }
        
        private List<string> GetDefaultScriptingDefines()
        {
            List<string> defines = new List<string>();
            
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out string[] scriptingDefines);
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
#if UNITY_STANDALONE_WIN
            return "{projectName}.exe"; // For Windows, the executable is a .exe file
#elif UNITY_MAC
            return "{projectName}.app"; // For macOS, the executable is a .app bundle
#else
            return "{projectName}"; // Default for other platforms
#endif
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
                { "EnableDeepProfilingSupport", EnableDeepProfilingSupport }
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
        }
    }
}