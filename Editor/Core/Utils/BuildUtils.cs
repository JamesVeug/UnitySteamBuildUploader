using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Wireframe
{
    public static class BuildUtils
    {
        public enum Architecture
        {
            Unknown,
            x86_64,
            x86_32,
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
            // 0 - None
            // 1 - ARM64
            // 2 - Universal (I'm assuming this is 32 bit)
#if UNITY_2021_1_OR_NEWER
            int architecture = PlayerSettings.GetArchitecture(NamedBuildTarget.FromBuildTargetGroup(BuildTargetToPlatform()));
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
    }
}