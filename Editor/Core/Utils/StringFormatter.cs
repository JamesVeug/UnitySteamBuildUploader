using System;
using System.Collections.Generic;
using System.IO;
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
            public Func<string> TaskProfileName { get; set; }
            public Func<string> TaskDescription { get; set; }
            public Func<string> UploadTaskFailText { get; set; }
            public Func<string> BuildName { get; set; }

            private Context parent;

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
        }
        
        internal static List<Command> Commands { get; } = new List<Command>
        {
            new Command("{projectName}", (ctx) => PlayerSettings.productName, "The name of your product as specified in Player Settings."),
            new Command("{bundleVersion}", (ctx) => PlayerSettings.bundleVersion, "The version of your project as specified in Player Settings."),
            new Command("{companyName}", (ctx) => PlayerSettings.companyName, "The name of your company as specified in Player Settings."),
            
            new Command("{activeBuildTarget}", (ctx) => EditorUserBuildSettings.activeBuildTarget.ToString(), "Which platform targeting for the next build as defined in Build Settings."),
            new Command("{activeBuildTargetGroup}", (ctx) => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString(), "The target group of the upcoming build as defined in Player Settings."),
            new Command("{activeScriptingBackend}", (ctx) => ScriptingBackend, "The scripting backend for the next build as defined in Player Settings."),
            
            new Command("{projectPath}", (ctx) => Path.GetDirectoryName(Application.dataPath), "The version of your project as specified in Player Settings."),
            new Command("{persistentDataPath}", (ctx) => Application.persistentDataPath, "The version of your project as specified in Player Settings."),
            new Command("{version}", (ctx) => Application.version, "The version of your project as specified in Player Settings."),
            new Command("{unityVersion}", (ctx) => Application.unityVersion, "The version of Unity you are using."),
            
            new Command("{date}", (ctx) => DateTime.Now.ToString("yyyy-MM-dd"), "The current local date in the format YYYY-MM-DD."),
            new Command("{time}", (ctx) => DateTime.Now.ToString("HH-mm-ss"), "The current local time in the format HH-MM-SS."),
            new Command("{dateTime}", (ctx) => DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."),
            
            new Command("{machineName}", (ctx) => Environment.MachineName, "The name of the machine running the build."),
            
            new Command("{taskProfileName}", (ctx) => ctx.TaskProfileName(), "The name of the upload profile or task specified when creating the task."),
            new Command("{taskDescription}", (ctx) => ctx.TaskDescription(), "The description of the current task being executed."),
            new Command("{taskFailedReasons}", (ctx) => ctx.UploadTaskFailText(), "Gets the reasons why the task failed to upload all destinations."),
            
            new Command("{buildName}", (ctx) => ctx.BuildName(), "The name of the build as specified in a build config."),
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
                    case ScriptingImplementation.CoreCLR:
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