using System;
using System.Collections.Generic;
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
            public Func<string> TaskProfileName { get; set; } = ()=> "<TaskProfileName not specified>";
            public Func<string> TaskDescription { get; set; } = ()=> "<TaskDescription not specified>";
            public Func<string> UploadTaskFailText { get; set; } = () => "<UploadTaskFailText not specified>";
        }
        
        internal static List<Command> Commands { get; } = new List<Command>
        {
            new Command("{projectName}", (ctx) => PlayerSettings.productName, "The name of your product as specified in Player Settings."),
            new Command("{bundleVersion}", (ctx) => PlayerSettings.bundleVersion, "The version of your project as specified in Player Settings."),
            new Command("{companyName}", (ctx) => PlayerSettings.companyName, "The name of your company as specified in Player Settings."),
            
            new Command("{activeBuildTarget}", (ctx) => EditorUserBuildSettings.activeBuildTarget.ToString(), "Which platform targeting for the next build as defined in Build Settings."),
            new Command("{activeBuildTargetGroup}", (ctx) => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString(), "The target group of the upcoming build as defined in Player Settings."),
            new Command("{activeScriptingBackend}", (ctx) => ScriptingBackend, "The scripting backend for the next build as defined in Player Settings."),
            
            new Command("{version}", (ctx) => Application.version, "The version of your project as specified in Player Settings."),
            new Command("{unityVersion}", (ctx) => Application.unityVersion, "The version of Unity you are using."),
            new Command("{date}", (ctx) => DateTime.Now.ToString("yyyy-MM-dd"), "The current local date in the format YYYY-MM-DD."),
            new Command("{time}", (ctx) => DateTime.Now.ToString("HH-mm-ss"), "The current local time in the format HH-MM-SS."),
            new Command("{dateTime}", (ctx) => DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."),
            new Command("{machineName}", (ctx) => Environment.MachineName, "The name of the machine running the build."),
            
            new Command("{taskProfileName}", (ctx) => ctx.TaskProfileName(), "The name of the upload profile or task specified when creating the task."),
            new Command("{taskDescription}", (ctx) => ctx.TaskDescription(), "The description of the current task being executed."),
            new Command("{taskFailedReasons}", (ctx) => ctx.UploadTaskFailText(), "Gets the reasons why the task failed to upload all destinations."),
        };

        private static string ScriptingBackend
        {
            get
            {
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
#if UNITY_2021_0_OR_NEWER
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

            foreach (var command in Commands)
            {
                format = Utils.Replace(format, command.Key, command.Formatter(context), StringComparison.OrdinalIgnoreCase);
            }

            return format;
        }
    }
}