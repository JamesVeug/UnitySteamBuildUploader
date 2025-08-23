﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static class BuildConfigsUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/BuildConfigs.json";

        public class BuildConfigPopup : CustomDropdown<BuildConfig>
        {
            public override string FirstEntryText => "Choose Build Config";

            protected override List<BuildConfig> FetchAllData()
            {
                GetBuildConfigs();
                return data;
            }
        }

        private static List<BuildConfig> data = null;

        public static List<BuildConfig> GetBuildConfigs()
        {
            if (data == null)
            {
                LoadFile();
            }
            return data;
        }

		[MenuItem("Tools/Build Uploader/Reload Build Configs")]
        private static void LoadFile()
        {
            data = new List<BuildConfig>();
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                SaveData savedData = JSON.DeserializeObject<SaveData>(json);
                if (savedData != null && savedData.Configs != null && savedData.Configs.Count > 0)
                {
                    int id = 1;
                    for (var i = 0; i < savedData.Configs.Count; i++)
                    {
                        var saveData = savedData.Configs[i];
                        BuildConfig config = new BuildConfig();
                        try
                        {
                            config.Deserialize(saveData);
                            config.Id = id++;
                            data.Add(config);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }

                    return;
                }
            }

            CreateDefaultConfigs();
            Save();
        }

        private static void CreateDefaultConfigs()
        {
            BuildConfig debugBuild = new BuildConfig();
            debugBuild.SetupDefaults();
            debugBuild.BuildName = "Debug Build";
            debugBuild.IsDevelopmentBuild = true;
            debugBuild.AllowDebugging = true;
            debugBuild.EnableDeepProfilingSupport = true;

            BuildConfig releaseBuild = new BuildConfig();
            releaseBuild.SetupDefaults();
            releaseBuild.BuildName = "Release Build";
#if UNITY_2021_0_OR_NEWER
            releaseBuild.StrippingLevel = ManagedStrippingLevel.Minimal;
#else
            releaseBuild.StrippingLevel = ManagedStrippingLevel.Low;
#endif
            releaseBuild.StackTraceLogTypes[LogType.Log] = StackTraceLogType.None;
            releaseBuild.StackTraceLogTypes[LogType.Warning] = StackTraceLogType.None;
            releaseBuild.StackTraceLogTypes[LogType.Error] = StackTraceLogType.ScriptOnly;
            releaseBuild.StackTraceLogTypes[LogType.Exception] = StackTraceLogType.ScriptOnly;
            releaseBuild.StackTraceLogTypes[LogType.Assert] = StackTraceLogType.None;

            
            data.Add(debugBuild);
            data.Add(releaseBuild);
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
                
                SaveData saveData = new SaveData();
                saveData.Version = SaveData.CurrentVersion;
                saveData.Configs = new List<Dictionary<string, object>>();
                foreach (BuildConfig config in data)
                {
                    Dictionary<string, object> serializedData = config.Serialize();
                    if (serializedData == null)
                    {
                        Debug.LogWarning("BuildConfig data is null. Skipping this config.");
                        continue;
                    }
                    
                    saveData.Configs.Add(serializedData);
                }

                string json = JSON.SerializeObject(saveData);
                File.WriteAllText(FilePath, json);
            }
        }

        public static BuildConfigPopup BuildConfigsPopup => m_buildConfigsPopup ?? (m_buildConfigsPopup = new BuildConfigPopup());
        private static BuildConfigPopup m_buildConfigsPopup;

        public class SaveData
        {
            public const int CurrentVersion = 1;
            
            public int Version;
            public List<Dictionary<string, object>> Configs = new List<Dictionary<string, object>>();
        }
    }
}