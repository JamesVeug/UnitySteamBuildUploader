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
        public const string BUILD_ARCHITECTURE_KEY = "{buildArchitecture}";
        public const string BUILD_TARGET_GROUP_KEY = "{buildTargetGroup}";
        public const string SCRIPTING_BACKEND_KEY = "{scriptingBackend}";
        public const string PROJECT_PATH_KEY = "{projectPath}";
        public const string PERSISTENT_DATA_PATH_KEY = "{persistentDataPath}";
        public const string CACHE_FOLDER_KEY = "{cacheFolderPath}";
        public const string UNITY_VERSION_KEY = "{unityVersion}";
        public const string DATE_KEY = "{date}";
        public const string TIME_KEY = "{time}";
        public const string DATE_TIME_KEY = "{dateTime}";
        public const string MACHINE_NAME_KEY = "{machineName}";
        public const string TASK_PROFILE_NAME_KEY = "{taskProfileName}";
        public const string TASK_DESCRIPTION_KEY = "{taskDescription}";
        public const string TASK_FAILED_REASONS_KEY = "{taskFailedReasons}";
        public const string UPLOAD_NUMBER_KEY = "{uploadNumber}";
        
        // Sources
        public const string BUILD_NAME_KEY = "{buildName}";
        public const string BUILD_NUMBER_KEY = "{buildNumber}";
        
        // Destinations
        public const string DESTINATION_LOCAL_PATH_KEY = "{destLocalPath}";
        public const string STEAM_APP_NAME_KEY = "{steamAppName}";
        public const string STEAM_BRANCH_NAME_KEY = "{steamBranchName}";
        public const string STEAM_DEPOT_NAME_KEY = "{steamDepotName}";
        public const string ITCHIO_USER_NAME_KEY = "{itchioUserName}";
        public const string ITCHIO_GAME_NAME_KEY = "{itchioGameName}";
        public const string ITCHIO_CHANNEL_NAME_KEY = "{itchioChannelName}";
        public const string EPIC_GAMES_ORGANIZATION_NAME_KEY = "{epicgamesOrganizationName}";
        public const string EPIC_GAMES_PRODUCT_NAME_KEY = "{epicgamesProductName}";
        public const string EPIC_GAMES_ARTIFACT_NAME_KEY = "{epicgamesArtifactName}";
        
        // Versions
        public const string VERSION_KEY = "{version}";
        public const string VERSION_MAJOR_KEY = "{versionMajor}";
        public const string VERSION_MINOR_KEY = "{versionMinor}";
        public const string VERSION_PATCH_KEY = "{versionPatch}";
        public const string VERSION_REVISION_KEY = "{versionRevision}";
        
        public const string VERSION_SEM_KEY = "{versionSem}";
        public const string VERSION_MAJOR_SEM_KEY = "{versionMajorSem}";
        public const string VERSION_MINOR_SEM_KEY = "{versionMinorSem}";
        public const string VERSION_PATCH_SEM_KEY = "{versionPatchSem}";
        public const string VERSION_REVISION_SEM_KEY = "{versionRevisionSem}";
        
        static StringFormatter()
        {
            Commands = new List<Command>();
            Commands.Add(new Command(PRODUCT_NAME_KEY, nameof(Context.ProductName), "The name of your product as specified in Player Settings."));
            Commands.Add(new Command(BUNDLE_VERSION_KEY, nameof(Context.BundleVersion), "The version of your project for android/ios as specified in Player Settings."));
            Commands.Add(new Command(COMPANY_NAME_KEY, nameof(Context.CompanyName), "The name of your company as specified in Player Settings."));
            Commands.Add(new Command(BUILD_TARGET_KEY, nameof(Context.buildTarget), "Which platform targeting for the next build as defined in Build Settings."));
            Commands.Add(new Command(BUILD_ARCHITECTURE_KEY, nameof(Context.buildArchitecture), "Which architecture the build will be (eg x64/x84)"));
            Commands.Add(new Command(BUILD_TARGET_GROUP_KEY, nameof(Context.buildTargetGroup), "The target group of the upcoming build as defined in Player Settings."));
            Commands.Add(new Command(SCRIPTING_BACKEND_KEY, nameof(Context.scriptingBackend), "The scripting backend for the next build as defined in Player Settings."));
            Commands.Add(new Command(PROJECT_PATH_KEY, nameof(Context.ProjectPath), "The path of your Unity Project contains the Assets folder."));
            Commands.Add(new Command(PERSISTENT_DATA_PATH_KEY, nameof(Context.PersistentDataPath), "The path of the Persistent Data folder"));
            Commands.Add(new Command(CACHE_FOLDER_KEY, nameof(Context.CacheFolderPath), "The path where all files and builds are stored when build uploader is working."));
            Commands.Add(new Command(UNITY_VERSION_KEY, nameof(Context.UnityVersion), "The version of Unity you are using."));
            Commands.Add(new Command(DATE_KEY, nameof(Context.Date), "The current local date in the format YYYY-MM-DD."));
            Commands.Add(new Command(TIME_KEY, nameof(Context.Time), "The current local time in the format HH-MM-SS."));
            Commands.Add(new Command(DATE_TIME_KEY, nameof(Context.DateTime), "The current local date and time in the format YYYY-MM-DD HH-MM-SS."));
            Commands.Add(new Command(MACHINE_NAME_KEY, nameof(Context.MachineName), "The name of the machine running the build."));
            Commands.Add(new Command(TASK_PROFILE_NAME_KEY, nameof(Context.TaskProfileName), "The name of the upload profile or task specified when creating the task."));
            Commands.Add(new Command(TASK_DESCRIPTION_KEY, nameof(Context.TaskDescription), "The description of the current task being executed."));
            Commands.Add(new Command(TASK_FAILED_REASONS_KEY, nameof(Context.UploadTaskFailText), "Gets the reasons why the task failed to upload all destinations."));
            Commands.Add(new Command(UPLOAD_NUMBER_KEY, nameof(Context.UploadNumber), "A unique number of the upload task that's getting sources and uploading them."));
            
            // Sources
            Commands.Add(new Command(BUILD_NAME_KEY, nameof(Context.BuildName), "The name of the build as specified in a build config."));
            Commands.Add(new Command(BUILD_NUMBER_KEY, nameof(Context.BuildNumber), "A unique number of the build that is produced."));
            
            // Destinations
            Commands.Add(new Command(DESTINATION_LOCAL_PATH_KEY, nameof(Context.DestinationLocalPath), "The path which files will be copied to using the LocalPath destination."));
            Commands.Add(new Command(STEAM_APP_NAME_KEY, nameof(Context.SteamAppName), "The name of the app that is being uploaded to on Steamworks."));
            Commands.Add(new Command(STEAM_BRANCH_NAME_KEY, nameof(Context.SteamBranchName), "The name of the branch that we are uploading to on Steamworks."));
            Commands.Add(new Command(STEAM_DEPOT_NAME_KEY, nameof(Context.SteamDepotName), "The name of the depots that are being uploaded to on Steamworks."));
            Commands.Add(new Command(ITCHIO_USER_NAME_KEY, nameof(Context.ItchioUserName), "The name of the user that owns the game that we want to upload the files to on Itchio."));
            Commands.Add(new Command(ITCHIO_GAME_NAME_KEY, nameof(Context.ItchioGameName), "The name of the game that is being uploaded to on Itchio."));
            Commands.Add(new Command(ITCHIO_CHANNEL_NAME_KEY, nameof(Context.ItchioChannelName), "The channels/platforms that we are uploading to on Itchio."));
            Commands.Add(new Command(EPIC_GAMES_ORGANIZATION_NAME_KEY, nameof(Context.EpicGamesOrganizationName), "The name of the organization that owns the product on Epic Games that will receive the files."));
            Commands.Add(new Command(EPIC_GAMES_PRODUCT_NAME_KEY, nameof(Context.EpicGamesProductName), "The name of the product that is being uploaded to on Epic Games."));
            Commands.Add(new Command(EPIC_GAMES_ARTIFACT_NAME_KEY, nameof(Context.EpicGamesArtifactName), "The name of the artifact that is being uploaded to on Epic Games."));
            
            // Versions
            Commands.Add(new Command(VERSION_KEY, nameof(Context.Version), "The version of your project as specified in Player Settings."));
            Commands.Add(new Command(VERSION_MAJOR_KEY, nameof(Context.VersionMajor), "The version of your project as specified in Player Settings but only the major segment. eg: a1 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_MINOR_KEY, nameof(Context.VersionMinor), "The version of your project as specified in Player Settings but only the minor segment. eg: 2 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_PATCH_KEY, nameof(Context.VersionPatch), "The version of your project as specified in Player Settings but only the patch segment. eg: 3 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_REVISION_KEY, nameof(Context.VersionRevision), "The version of your project as specified in Player Settings but only the revision segment. eg: beta1 from a1.2.3-beta1"));
            
            Commands.Add(new Command(VERSION_SEM_KEY, nameof(Context.VersionSem), "The version of your project as specified in Player Settings but with only numbers. eg: 1.2.3 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_MAJOR_SEM_KEY, nameof(Context.VersionMajorSem), "The major segment of the version of your project as specified in Player Settings but with only numbers. eg: 1 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_MINOR_SEM_KEY, nameof(Context.VersionMinorSem), "The minor segment of the version of your project as specified in Player Settings but with only numbers. eg: 2 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_PATCH_SEM_KEY, nameof(Context.VersionPatchSem), "The patch segment of the version of your project as specified in Player Settings but with only numbers. eg: 3 from a1.2.3-beta1"));
            Commands.Add(new Command(VERSION_REVISION_SEM_KEY, nameof(Context.VersionRevisionSem), "The revision segment of the version of your project as specified in Player Settings but with only numbers. eg: 1 from a1.2.3-beta1"));
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
            bool ReplaceString(string key, out string value, Context ctx);
        }

        public class Context
        {
            private class DoNotCacheAttribute : Attribute { }
            
            public Func<string> ProductName { get; set; } = ()=> PlayerSettings.productName;
            public Func<string> BundleVersion { get; set; } = ()=> PlayerSettings.bundleVersion;
            public Func<string> CompanyName { get; set; } = ()=> PlayerSettings.companyName;
            public Func<string> UploadNumber { get; set; } = ()=> (BuildUploaderProjectSettings.Instance.TotalUploadTasksStarted + 1).ToString();
            
            public Func<string> buildTarget { get; set; } = ()=> EditorUserBuildSettings.activeBuildTarget.ToString();
            public Func<string> buildArchitecture { get; set; } = ()=> BuildUtils.CurrentTargetArchitecture().ToString();
            public Func<string> buildTargetGroup { get; set; } = ()=> BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString();
            public Func<string> scriptingBackend { get; set; } = ()=> BuildUtils.ScriptingBackendDisplayName(BuildUtils.CurrentScriptingBackend());
            public Func<string> ProjectPath { get; set; } = ()=> Path.GetDirectoryName(Application.dataPath);
            public Func<string> PersistentDataPath { get; set; } = ()=> Application.persistentDataPath;
            public Func<string> CacheFolderPath { get; set; } = ()=> Preferences.CacheFolderPath;
            public Func<string> UnityVersion { get; set; } = ()=> Application.unityVersion;
            public Func<string> Date { get; set; } = ()=> System.DateTime.Now.ToString("yyyy-MM-dd");
            public Func<string> Time { get; set; } = ()=> System.DateTime.Now.ToString("HH-mm-ss");
            public Func<string> DateTime { get; set; } = ()=> System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            public Func<string> MachineName { get; set; } = ()=> Environment.MachineName;
            
            // Versions
            public Func<string> Version { get; set; } = ()=> Application.version;
            public Func<string> VersionMajor { get; set; } = ()=> Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Major);
            public Func<string> VersionMinor { get; set; } = ()=> Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Minor);
            public Func<string> VersionPatch { get; set; } = ()=> Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Patch);
            public Func<string> VersionRevision { get; set; } = ()=> Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Revision);
            public Func<string> VersionSem { get; set; } = ()=> Utils.ToSemantic(Application.version);
            public Func<string> VersionMajorSem { get; set; } = ()=> Utils.ToSemantic(Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Major));
            public Func<string> VersionMinorSem { get; set; } = ()=> Utils.ToSemantic(Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Minor));
            public Func<string> VersionPatchSem { get; set; } = ()=> Utils.ToSemantic(Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Patch));
            public Func<string> VersionRevisionSem { get; set; } = ()=> Utils.ToSemantic(Utils.VersionSegmentToString(Application.version, Utils.VersionSegment.Revision));
            
            [DoNotCache] public Func<string> TaskProfileName { get; set; }
            [DoNotCache] public Func<string> TaskDescription { get; set; }
            [DoNotCache] public Func<string> UploadTaskFailText { get; set; }
            
            // Sources
            [DoNotCache] public Func<string> BuildName { get; set; }
            [DoNotCache] public Func<string> BuildNumber { get; set; }
            
            // Destinations
            [DoNotCache] public Func<string> DestinationLocalPath { get; set; }
            [DoNotCache] public Func<string> SteamAppName { get; set; }
            [DoNotCache] public Func<string> SteamBranchName { get; set; }
            [DoNotCache] public Func<string> SteamDepotName { get; set; }
            [DoNotCache] public Func<string> ItchioUserName { get; set; }
            [DoNotCache] public Func<string> ItchioGameName { get; set; }
            [DoNotCache] public Func<string> ItchioChannelName { get; set; }
            [DoNotCache] public Func<string> EpicGamesOrganizationName { get; set; }
            [DoNotCache] public Func<string> EpicGamesProductName { get; set; }
            [DoNotCache] public Func<string> EpicGamesArtifactName { get; set; }
            
            

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
                
                DestinationLocalPath = () => "<DestinationLocalPath>";
                SteamAppName = () => "<SteamAppName>";
                SteamBranchName = () => "<SteamBranchName>";
                SteamDepotName = () => "<SteamDepotName>";
                ItchioUserName = () => "<ItchioUserName>";
                ItchioGameName = () => "<ItchioGameName>";
                ItchioChannelName = () => "<ItchioChannelName>";
                EpicGamesOrganizationName = () => "<EpicGamesOrganizationName>";
                EpicGamesProductName = () => "<EpicGamesProductName>";
                EpicGamesArtifactName = () => "<EpicGamesArtifactName>";
            }
            
            public void SetParent(Context context)
            {
                parent = context;
            }

            internal string Get(string key, string fieldName, Func<string> getter)
            {
                foreach (IContextModifier modifier in modifiers)
                {
                    if (modifier.ReplaceString(key, out string value, this))
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