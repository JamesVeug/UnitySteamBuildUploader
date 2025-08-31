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
        internal static Command PRODUCT_NAME;
        internal static Command BUNDLE_VERSION;
        internal static Command COMPANY_NAME;
        internal static Command BUILD_TARGET;
        internal static Command BUILD_TARGET_GROUP;
        internal static Command SCRIPTING_BACKEND;
        internal static Command PROJECT_PATH;
        internal static Command PERSISTENT_DATA_PATH;
        internal static Command VERSION;
        internal static Command UNITY_VERSION;
        internal static Command DATE;
        internal static Command TIME;
        internal static Command DATE_TIME;
        internal static Command MACHINE_NAME;
        internal static Command TASK_PROFILE_NAME;
        internal static Command TASK_DESCRIPTION;
        internal static Command TASK_FAILED_REASONS;
        internal static Command BUILD_NAME;
        
        static StringFormatter()
        {
            Commands = new List<Command>();
            PRODUCT_NAME =          AddToList(new Command("{productName}", nameof(Context.ProjectName),(ctx) => ctx.ProjectName, "The name of your product as specified in Player Settings."));
            BUNDLE_VERSION =        AddToList(new Command("{bundleVersion}", nameof(Context.BundleVersion),(ctx) => ctx.BundleVersion, "The version of your project as specified in Player Settings."));
            COMPANY_NAME =          AddToList(new Command("{companyName}", nameof(Context.CompanyName),(ctx) => ctx.CompanyName, "The name of your company as specified in Player Settings."));
            BUILD_TARGET =          AddToList(new Command("{buildTarget}", nameof(Context.buildTarget),(ctx) => ctx.buildTarget, "Which platform targeting for the next build as defined in Build Settings."));
            BUILD_TARGET_GROUP =    AddToList(new Command("{buildTargetGroup}", nameof(Context.buildTargetGroup),(ctx) => ctx.buildTargetGroup, "The target group of the upcoming build as defined in Player Settings."));
            SCRIPTING_BACKEND =     AddToList(new Command("{scriptingBackend}", nameof(Context.scriptingBackend),(ctx) => ctx.scriptingBackend, "The scripting backend for the next build as defined in Player Settings."));
            PROJECT_PATH =          AddToList(new Command("{projectPath}", nameof(Context.ProjectPath),(ctx) => ctx.ProjectPath, "The version of your project as specified in Player Settings."));
            PERSISTENT_DATA_PATH =  AddToList(new Command("{persistentDataPath}", nameof(Context.PersistentDataPath),(ctx) => ctx.PersistentDataPath, "The version of your project as specified in Player Settings."));
            VERSION =               AddToList(new Command("{version}", nameof(Context.Version),(ctx) => ctx.Version, "The version of your project as specified in Player Settings."));
            UNITY_VERSION =         AddToList(new Command("{unityVersion}", nameof(Context.UnityVersion),(ctx) => ctx.UnityVersion, "The version of Unity you are using."));
            DATE =                  AddToList(new Command("{date}", nameof(Context.Date),(ctx) => ctx.Date, "The current local date in the format YYYY-MM-DD."));
            TIME =                  AddToList(new Command("{time}", nameof(Context.Time),(ctx) => ctx.Time, "The current local time in the format HH-MM-SS."));
            DATE_TIME =             AddToList(new Command("{dateTime}", nameof(Context.DateTime),(ctx) => ctx.DateTime, "The current local date and time in the format YYYY-MM-DD HH-MM-SS."));
            MACHINE_NAME =          AddToList(new Command("{machineName}", nameof(Context.MachineName),(ctx) => ctx.MachineName, "The name of the machine running the build."));
            TASK_PROFILE_NAME =     AddToList(new Command("{taskProfileName}", nameof(Context.TaskProfileName),(ctx) => ctx.TaskProfileName, "The name of the upload profile or task specified when creating the task."));
            TASK_DESCRIPTION =      AddToList(new Command("{taskDescription}", nameof(Context.TaskDescription),(ctx) => ctx.TaskDescription, "The description of the current task being executed."));
            TASK_FAILED_REASONS =   AddToList(new Command("{taskFailedReasons}", nameof(Context.UploadTaskFailText),(ctx) => ctx.UploadTaskFailText, "Gets the reasons why the task failed to upload all destinations."));
            BUILD_NAME =            AddToList(new Command("{buildName}", nameof(Context.BuildName),(ctx) => ctx.BuildName, "The name of the build as specified in a build config."));

            Command AddToList(Command command)
            {
                Commands.Add(command);
                return command;
            }
        }

        internal class Command
        {
            public string Key { get; }
            public string FieldName { get; }
            public string Tooltip { get; }
            public Func<Context, Func<string>> Formatter { get; }
            
            public Command(string key, string fieldName, Func<Context, Func<string>> formatter, string tooltip)
            {
                Key = key;
                FieldName = fieldName;
                Tooltip = tooltip;
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
            
            public Func<string> ProjectName { get; set; } = ()=> PlayerSettings.productName;
            public Func<string> BundleVersion { get; set; } = ()=> PlayerSettings.bundleVersion;
            public Func<string> CompanyName { get; set; } = ()=> PlayerSettings.companyName;
            
            public Func<string> buildTarget { get; set; } = ()=> EditorUserBuildSettings.activeBuildTarget.ToString();
            public Func<string> buildTargetGroup { get; set; } = ()=> BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString();
            public Func<string> scriptingBackend { get; set; } = ()=> ScriptingBackend;
            public Func<string> ProjectPath { get; set; } = ()=> Path.GetDirectoryName(Application.dataPath);
            public Func<string> PersistentDataPath { get; set; } = ()=> Application.persistentDataPath;
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
            

            private Context parent;
            private Dictionary<string, string> cachedValues = new Dictionary<string, string>();
            private List<IContextModifier> modifiers = new List<IContextModifier>();

            public Context()
            {
                TaskProfileName = ()=> parent != null ? parent.TaskProfileName() : "<TaskProfileName>";
                TaskDescription = ()=> parent != null ? parent.TaskDescription() : "<TaskDescription>";
                UploadTaskFailText = () => parent != null ? parent.UploadTaskFailText() : "<UploadTaskFailText>";
                BuildName = () => "<BuildName>";
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
        

        private static string ScriptingBackend
        {
            get
            {
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
#if UNITY_2021_1_OR_NEWER
                NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                ScriptingImplementation implementation = PlayerSettings.GetScriptingBackend(namedBuildTarget);
#else
                ScriptingImplementation implementation = PlayerSettings.GetScriptingBackend(buildTargetGroup);
#endif

                switch (implementation)
                {
                    case ScriptingImplementation.IL2CPP:
                        return "IL2CPP";
                    case ScriptingImplementation.Mono2x:
                        return "Mono";
                    case ScriptingImplementation.WinRTDotNET:
                        return "DotNet";
#if UNITY_6000_0_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
                    case ScriptingImplementation.CoreCLR:
#pragma warning restore CS0618 // Type or member is obsolete
                        return "CoreCLR";
#endif
                    default:
                        return implementation.ToString(); // Unhandled like CoreCLR
                }
            }
        }

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