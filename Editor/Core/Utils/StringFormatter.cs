using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Wireframe
{
    public static class StringFormatter
    {
        public const string PRODUCT_NAME_KEY = "{productName}";
        public const string BUNDLE_VERSION_KEY = "{bundleVersion}";
        public const string COMPANY_NAME_KEY = "{companyName}";
        public const string BUILD_TARGET_KEY = "{buildTarget}";
        public const string BUILD_TARGET_GROUP_KEY = "{buildTargetGroup}";
        public const string SCRIPTING_BACKEND_KEY = "{scriptingBackend}";
        public const string PROJECT_PATH_KEY = "{projectPath}";
        public const string PERSISTENT_DATA_PATH_KEY = "{persistentDataPath}";
        public const string VERSION_KEY = "{version}";
        public const string CACHE_FOLDER_KEY = "{cacheFolderPath}";
        public const string UNITY_VERSION_KEY = "{unityVersion}";
        public const string DATE_KEY = "{date}";
        public const string TIME_KEY = "{time}";
        public const string DATE_TIME_KEY = "{dateTime}";
        public const string MACHINE_NAME_KEY = "{machineName}";
        public const string TASK_PROFILE_NAME_KEY = "{taskProfileName}";
        public const string TASK_DESCRIPTION_KEY = "{taskDescription}";
        public const string TASK_FAILED_REASONS_KEY = "{taskFailedReasons}";
        public const string BUILD_NAME_KEY = "{buildName}";
        public const string BUILD_NUMBER_KEY = "{buildNumber}";
        
        static StringFormatter()
        {
            Commands = new List<Command>();
            Commands.Add(new Command(PRODUCT_NAME_KEY, nameof(Context.ProductName), "The name of your product as specified in Player Settings."));
            Commands.Add(new Command(BUNDLE_VERSION_KEY, nameof(Context.BundleVersion), "The version of your project as specified in Player Settings."));
            Commands.Add(new Command(COMPANY_NAME_KEY, nameof(Context.CompanyName), "The name of your company as specified in Player Settings."));
            Commands.Add(new Command(BUILD_TARGET_KEY, nameof(Context.buildTarget), "Which platform targeting for the next build as defined in Build Settings."));
            Commands.Add(new Command(BUILD_TARGET_GROUP_KEY, nameof(Context.buildTargetGroup), "The target group of the upcoming build as defined in Player Settings."));
            Commands.Add(new Command(SCRIPTING_BACKEND_KEY, nameof(Context.scriptingBackend), "The scripting backend for the next build as defined in Player Settings."));
            Commands.Add(new Command(PROJECT_PATH_KEY, nameof(Context.ProjectPath), "The version of your project as specified in Player Settings."));
            Commands.Add(new Command(PERSISTENT_DATA_PATH_KEY, nameof(Context.PersistentDataPath), "The version of your project as specified in Player Settings."));
            Commands.Add(new Command(CACHE_FOLDER_KEY, nameof(Context.CacheFolderPath), "The path where all files and builds are stored when build uploader is working."));
            Commands.Add(new Command(VERSION_KEY, nameof(Context.Version), "The version of your project as specified in Player Settings."));
            Commands.Add(new Command(UNITY_VERSION_KEY, nameof(Context.UnityVersion), "The version of Unity you are using."));
            Commands.Add(new Command(DATE_KEY, nameof(Context.Date), "The current local date in the format YYYY-MM-DD."));
            Commands.Add(new Command(TIME_KEY, nameof(Context.Time), "The current local time in the format HH-MM-SS."));
            Commands.Add(new Command(DATE_TIME_KEY, nameof(Context.DateTime), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."));
            Commands.Add(new Command(MACHINE_NAME_KEY, nameof(Context.MachineName), "The name of the machine running the build."));
            Commands.Add(new Command(TASK_PROFILE_NAME_KEY, nameof(Context.TaskProfileName), "The name of the upload profile or task specified when creating the task."));
            Commands.Add(new Command(TASK_DESCRIPTION_KEY, nameof(Context.TaskDescription), "The description of the current task being executed."));
            Commands.Add(new Command(TASK_FAILED_REASONS_KEY, nameof(Context.UploadTaskFailText), "Gets the reasons why the task failed to upload all destinations."));
            Commands.Add(new Command(BUILD_NAME_KEY, nameof(Context.BuildName), "The name of the build as specified in a build config."));
            Commands.Add(new Command(BUILD_NUMBER_KEY, nameof(Context.BuildNumber), "The unique number of the build that is produced if enabled in Project Settings."));
        }

        internal class Command
        {
            public string Key { get; }
            public string FieldName { get; }
            public string Tooltip { get; }
            public Func<Context, Func<string>> Formatter { get; }
            
            public Command(string key, string fieldName, string tooltip)
            {
                Key = key;
                FieldName = fieldName;
                Tooltip = tooltip;

                // Yes yes i know this is ugly
                var info = typeof(Context).GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Func<Context, Func<string>> formatter = (Context ctx) => (Func<string>)info.GetValue(ctx);
                Formatter = formatter;
            }
        }
        
        public interface IContextModifier
        {
            public bool ReplaceString(string key, out string value);
        }

        public class Context
        {
            private class DoNotCacheAttribute : Attribute { }
            
            public Func<string> ProductName { get; set; } = ()=> PlayerSettings.productName;
            public Func<string> BundleVersion { get; set; } = ()=> PlayerSettings.bundleVersion;
            public Func<string> CompanyName { get; set; } = ()=> PlayerSettings.companyName;
            
            public Func<string> buildTarget { get; set; } = ()=> EditorUserBuildSettings.activeBuildTarget.ToString();
            public Func<string> buildTargetGroup { get; set; } = ()=> BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString();
            public Func<string> scriptingBackend { get; set; } = ()=> BuildUtils.ScriptingBackendDisplayName(BuildUtils.CurrentScriptingBackend());
            public Func<string> ProjectPath { get; set; } = ()=> Path.GetDirectoryName(Application.dataPath);
            public Func<string> PersistentDataPath { get; set; } = ()=> Application.persistentDataPath;
            public Func<string> CacheFolderPath { get; set; } = ()=> Preferences.CacheFolderPath;
            public Func<string> Version { get; set; } = ()=> Application.version;
            public Func<string> UnityVersion { get; set; } = ()=> Application.unityVersion;
            public Func<string> Date { get; set; } = ()=> System.DateTime.Now.ToString("yyyy-MM-dd");
            public Func<string> Time { get; set; } = ()=> System.DateTime.Now.ToString("HH-mm-ss");
            public Func<string> DateTime { get; set; } = ()=> System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            public Func<string> MachineName { get; set; } = ()=> Environment.MachineName;
            
            [DoNotCache] public Func<string> TaskProfileName { get; set; }
            [DoNotCache] public Func<string> TaskDescription { get; set; }
            [DoNotCache] public Func<string> UploadTaskFailText { get; set; }
            [DoNotCache] public Func<string> BuildName { get; set; }
            [DoNotCache] public Func<string> BuildNumber { get; set; }
            

            private Context parent;
            private Dictionary<string, string> cachedValues = new Dictionary<string, string>();
            private List<IContextModifier> modifiers = new List<IContextModifier>();

            public Context()
            {
                TaskProfileName = ()=> parent != null ? parent.TaskProfileName() : "<TaskProfileName>";
                TaskDescription = ()=> parent != null ? parent.TaskDescription() : "<TaskDescription>";
                UploadTaskFailText = () => parent != null ? parent.UploadTaskFailText() : "<UploadTaskFailText>";
                BuildName = () => "<BuildName>";
                BuildNumber = () => "<BuildNumber>";
            }
            
            public void SetParent(Context context)
            {
                parent = context;
            }

            internal string Get(string key, string fieldName, Func<string> getter)
            {
                foreach (IContextModifier modifier in modifiers)
                {
                    if (modifier.ReplaceString(key, out string value))
                    {
                        return value;
                    }
                }
                
                if (cachedValues.TryGetValue(fieldName, out string cachedValue))
                {
                    return cachedValue;
                }
                
                return getter();
            }

            public void CacheCallbacks()
            {
                // Use reflection to get all fields of type Func<string> and invoke them to cache their values in a dictionary
                // Yes this is really gross but in time a better solution will be made
                var fields = typeof(Context).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(DoNotCacheAttribute)))
                    {
                        continue;
                    }
                    
                    if (field.PropertyType == typeof(Func<string>))
                    {
                        var func = (Func<string>)field.GetValue(this);
                        if (func != null)
                        {
                            string value = func();
                            cachedValues[field.Name] = value;
                        }
                    }
                }
            }

            public void AddModifier(IContextModifier iContextModifier)
            {
                modifiers.Add(iContextModifier);
            }
        }
        
        internal static List<Command> Commands { get; }

        public static string FormatString(string format, Context context)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            try {
                foreach (var command in Commands)
                {
                    int index = Utils.IndexOf(format, command.Key, StringComparison.OrdinalIgnoreCase);
                    while (index >= 0)
                    {
                        string formattedValue = context.Get(command.Key, command.FieldName, command.Formatter(context));
                        format = Utils.Replace(format, command.Key, formattedValue, StringComparison.OrdinalIgnoreCase);
                        index = Utils.IndexOf(format, command.Key, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to format string: " + format);
                Debug.LogException(e);
            }

            return format;
        }
    }
}