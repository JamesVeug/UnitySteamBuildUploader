using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static class StringFormatter
    {
        internal class Command
        {
            public string Key { get; }
            public string Tooltip { get; }
            public Func<string> Formatter { get; }
            
            public Command(string key, Func<string> formatter, string tooltip)
            {
                Key = key;
                Tooltip = tooltip;
                Formatter = formatter;
            }
        }
        
        internal static List<Command> Commands { get; } = new List<Command>
        {
            new Command("{projectName}", () => PlayerSettings.productName, "The name of your product as specified in Player Settings."),
            new Command("{bundleVersion}", () => PlayerSettings.bundleVersion, "The version of your project as specified in Player Settings."),
            new Command("{companyName}", () => PlayerSettings.companyName, "The name of your company as specified in Player Settings."),
            new Command("{version}", () => Application.version, "The version of your project as specified in Player Settings."),
            new Command("{unityVersion}", () => Application.unityVersion, "The version of Unity you are using."),
            new Command("{date}", () => DateTime.Now.ToString("yyyy-MM-dd"), "The current local date in the format YYYY-MM-DD."),
            new Command("{time}", () => DateTime.Now.ToString("HH-mm-ss"), "The current local time in the format HH-MM-SS."),
            new Command("{dateTime}", () => DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."),
            new Command("{machineName}", () => Environment.MachineName, "The name of the machine running the build."),
            
        };
        
        public static string FormatString(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            foreach (var command in Commands)
            {
                format = Utils.Replace(format, command.Key, command.Formatter(), StringComparison.OrdinalIgnoreCase);
            }

            return format;
        }
    }
}