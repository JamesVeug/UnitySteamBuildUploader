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
        internal class Command
        {
            public string Key { get; }
            public string Tooltip { get; }
            public Func<Context, string> Formatter { get; }
            
            public Command(string key, Func<Context, string> formatter, string tooltip)
            {
                Key = key;
                Tooltip = tooltip;
                Formatter = formatter;
            }
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

            internal string Get(string key, Func<string> getter)
            {
                if (cachedValues.TryGetValue(key, out string cachedValue))
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
        }
        
        internal static List<Command> Commands { get; } = new List<Command>
        {
            new Command("{productName}", (ctx) => ctx.Get(nameof(ctx.ProjectName), ctx.ProjectName), "The name of your product as specified in Player Settings."),
            new Command("{bundleVersion}", (ctx) => ctx.Get(nameof(ctx.BundleVersion), ctx.BundleVersion), "The version of your project as specified in Player Settings."),
            new Command("{companyName}", (ctx) => ctx.Get(nameof(ctx.CompanyName), ctx.CompanyName), "The name of your company as specified in Player Settings."),
            
            new Command("{buildTarget}", (ctx) => ctx.Get(nameof(ctx.buildTarget), ctx.buildTarget), "Which platform targeting for the next build as defined in Build Settings."),
            new Command("{buildTargetGroup}", (ctx) => ctx.Get(nameof(ctx.buildTargetGroup), ctx.buildTargetGroup), "The target group of the upcoming build as defined in Player Settings."),
            new Command("{scriptingBackend}", (ctx) => ctx.Get(nameof(ctx.scriptingBackend), ctx.scriptingBackend), "The scripting backend for the next build as defined in Player Settings."),
            
            new Command("{projectPath}", (ctx) => ctx.Get(nameof(ctx.ProjectPath), ctx.ProjectPath), "The version of your project as specified in Player Settings."),
            new Command("{persistentDataPath}", (ctx) => ctx.Get(nameof(ctx.PersistentDataPath), ctx.PersistentDataPath), "The version of your project as specified in Player Settings."),
            new Command("{version}", (ctx) => ctx.Get(nameof(ctx.Version), ctx.Version), "The version of your project as specified in Player Settings."),
            new Command("{unityVersion}", (ctx) => ctx.Get(nameof(ctx.UnityVersion), ctx.UnityVersion), "The version of Unity you are using."),
            
            new Command("{date}", (ctx) => ctx.Get(nameof(ctx.Date), ctx.Date), "The current local date in the format YYYY-MM-DD."),
            new Command("{time}", (ctx) => ctx.Get(nameof(ctx.Time), ctx.Time), "The current local time in the format HH-MM-SS."),
            new Command("{dateTime}", (ctx) => ctx.Get(nameof(ctx.DateTime), ctx.DateTime), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."),
            
            new Command("{machineName}", (ctx) => ctx.Get(nameof(ctx.MachineName), ctx.MachineName), "The name of the machine running the build."),
            
            new Command("{taskProfileName}", (ctx) => ctx.Get(nameof(ctx.TaskProfileName), ctx.TaskProfileName), "The name of the upload profile or task specified when creating the task."),
            new Command("{taskDescription}", (ctx) => ctx.Get(nameof(ctx.TaskDescription), ctx.TaskDescription), "The description of the current task being executed."),
            new Command("{taskFailedReasons}", (ctx) => ctx.Get(nameof(ctx.UploadTaskFailText), ctx.UploadTaskFailText), "Gets the reasons why the task failed to upload all destinations."),
            
            new Command("{buildName}", (ctx) => ctx.Get(nameof(ctx.BuildName), ctx.BuildName), "The name of the build as specified in a build config."),
        };

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
                        format = Utils.Replace(format, command.Key, command.Formatter(context), StringComparison.OrdinalIgnoreCase);
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