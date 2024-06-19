using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace Wireframe
{
    public class SteamSDK
    {
        public static SteamSDK Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new SteamSDK();
                }

                return m_instance;
            }
        }

        public static string SteamSDKPath
        {
            get => PlayerPrefs.GetString("steambuild_SDKPath");
            set => PlayerPrefs.SetString("steambuild_SDKPath", value);
        }

        public static string UserName
        {
            get => EditorPrefs.GetString("steambuild_SDKUser");
            set => EditorPrefs.SetString("steambuild_SDKUser", value);
        }

        public static string UserPassword
        {
            get => EditorPrefs.GetString("steambuild_SDKPass");
            set => EditorPrefs.SetString("steambuild_SDKPass", value);
        }

        public bool IsInitialized => m_initialized;

        private static SteamSDK m_instance;

        private Process m_uploadProcess;
        private string m_scriptPath;
        private string m_contentPath;
        private string m_exePath;
        private bool m_initialized;

        private SteamSDK()
        {
            if (!string.IsNullOrEmpty(SteamSDKPath))
            {
                Initialize();
            }
        }

        ~SteamSDK()
        {
            if (m_uploadProcess != null)
                m_uploadProcess.Kill();
        }

        public void Initialize()
        {
            m_initialized = false;
            if (!Directory.Exists(SteamSDKPath))
            {
                return;
            }

            string contentBuilderPath = null;
            foreach (string directory in Directory.GetDirectories(SteamSDKPath, "*", SearchOption.AllDirectories))
            {
                if (directory.EndsWith("ContentBuilder"))
                {
                    contentBuilderPath = directory;
                    break;
                }
            }

            if (string.IsNullOrEmpty(contentBuilderPath))
            {
                Debug.LogError("Could not find ContentBuilder in sdk path!");
                return;
            }

            string content = null;
            string scripts = null;
            foreach (string directory in Directory.GetDirectories(contentBuilderPath, "*", SearchOption.AllDirectories))
            {
                if (directory.EndsWith("content"))
                {
                    content = directory;
                }
                else if (directory.EndsWith("scripts"))
                {
                    scripts = directory;
                }

                if (content != null && scripts != null)
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("Could not find content inside ContentBuilder path!");
                return;
            }

            if (string.IsNullOrEmpty(scripts))
            {
                Debug.LogError("Could not find scripts inside ContentBuilder path!");
                return;
            }

            string exePath = Path.Combine(contentBuilderPath, "builder", "steamcmd.exe");
            if (!File.Exists(exePath))
            {
                Debug.LogError("Could not find steamcmd.exe inside ContentBuilder/builder path!");
                return;
            }

            m_exePath = exePath;
            m_contentPath = content;
            m_scriptPath = scripts;
            m_initialized = true;
        }

        public async Task CreateAppFiles(AppVDFFile appFile, DepotVDFFile depot, string branch, string description,
            string sourceFilePath)
        {
            appFile.desc = description;
            appFile.buildoutput = "..\\output\\";
            appFile.contentroot = sourceFilePath;
            appFile.depots.Clear();
            appFile.depots.Add(depot.DepotID, string.Format("depot_build_{0}.vdf", depot.DepotID));
            appFile.setlive = branch == "none" ? "" : branch;

            string fullPath = GetAppScriptOutputPath(appFile);
            await VDFFile.Save(appFile, fullPath);
        }

        public async Task CreateDepotFiles(DepotVDFFile depot)
        {
            depot.FileExclusion = "*.pdb";
            depot.FileMapping = new DepotFileMapping
            {
                LocalPath = ".\\*",
                DepotPath = ".",
                recursive = true
            };

            string fullPath = GetDepotScriptOutputPath(depot);
            await VDFFile.Save(depot, fullPath);
        }

        public string GetAppScriptOutputPath(AppVDFFile appFile)
        {
            string fileName = string.Format("app_build_{0}.vdf", appFile.appid);
            string fullPath = Path.Combine(m_scriptPath, fileName);
            return fullPath;
        }

        public string GetDepotScriptOutputPath(DepotVDFFile depot)
        {
            string fileName = string.Format("depot_build_{0}.vdf", depot.DepotID);
            string fullPath = Path.Combine(m_scriptPath, fileName);
            return fullPath;
        }

        public async Task<bool> Upload(AppVDFFile appFile)
        {
            bool result = false;
            try
            {
                m_uploadProcess = new Process();
                m_uploadProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                m_uploadProcess.StartInfo.CreateNoWindow = true;
                m_uploadProcess.StartInfo.UseShellExecute = false;
                m_uploadProcess.StartInfo.FileName = m_exePath;
                m_uploadProcess.StartInfo.Arguments = CreateSteamArguments(appFile);
                m_uploadProcess.StartInfo.RedirectStandardError = true;
                m_uploadProcess.StartInfo.RedirectStandardOutput = true;
                m_uploadProcess.EnableRaisingEvents = true;
                m_uploadProcess.Start();
                string textDump = await m_uploadProcess.StandardOutput.ReadToEndAsync();
                Debug.Log(textDump);
                result = LogOutSteamResult(textDump);
                m_uploadProcess.WaitForExit();
                m_uploadProcess.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                result = false;
            }

            await Task.Delay(10);
            return result;
        }

        private string CreateSteamArguments(AppVDFFile appFile)
        {
            string fullDirectory = GetAppScriptOutputPath(appFile);
            string arguments = string.Format("+login \"{0}\" \"{1}\" +run_app_build \"{2}\" +quit", UserName,
                UserPassword, fullDirectory);
            return arguments;
        }

        private bool LogOutSteamResult(string text)
        {
            int errorTextStartIndex = text.IndexOf("Error!", StringComparison.CurrentCultureIgnoreCase);
            if (errorTextStartIndex >= 0)
            {
                int errorNewLine = text.IndexOf('\n', errorTextStartIndex);
                string errorText = text.Substring(errorTextStartIndex, errorNewLine - errorTextStartIndex);
                Debug.LogError("[STEAM] " + errorText);
                return false;
            }

            string[] lines = text.Split('\n');
            int index = -1;
            
            if (!ContainsText(lines, "Loading Steam API", "OK", out index))
            {
                Debug.LogError("[STEAM] Failed to load API.");
                return false;
            }

            if (!ContainsText(lines, "Logging in user", "OK", out index))
            {
                string context = "";
                if (index != -1)
                {
                    context = lines[index + 1];
                }

                if (string.IsNullOrEmpty(context))
                {
                    Debug.LogError("[STEAM] Failed to log into User account: " + context);
                }
                else
                {
                    Debug.LogError("[STEAM] Failed to log into User account: ");
                }

                return false;
            }

            if (!ContainsText(lines, "Uploading content...", "", out index))
            {
                Debug.LogError("[STEAM] Failed to scan content to upload...");
                return false;
            }

            if (text.Contains("Fail"))
            {
                Debug.LogError("[STEAM] Failed to upload to steam. Check logs for info!");
                return false;
            }

            Debug.Log("[STEAM] Upload successful.");
            return true;
        }

        private bool ContainsText(string[] lines, string startsWith, string endsWith, out int startsWithIndex)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith(startsWith))
                {
                    bool success = string.IsNullOrEmpty(endsWith) || line.EndsWith(endsWith);
                    startsWithIndex = i;
                    return true;
                }
            }

            startsWithIndex = -1;
            return false;
        }

        public void ShowConsole()
        {
            var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.FileName = m_exePath;
            process.StartInfo.Arguments = "";
            process.EnableRaisingEvents = true;
            process.Start();
        }
    }
}