using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
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

        public string SteamSDKPath
        {
            get => PlayerPrefs.GetString("steambuild_SDKPath");
            set => PlayerPrefs.SetString("steambuild_SDKPath", value);
        }

        public string UserName
        {
            get => EditorPrefs.GetString("steambuild_SDKUser");
            set => EditorPrefs.SetString("steambuild_SDKUser", value);
        }

        public string UserPassword
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

        public IEnumerator CreateAppFiles(AppVDFFile appFile, DepotVDFFile depot, string branch, string description,
            string sourceFilePath)
        {
            appFile.desc = description;
            appFile.buildoutput = "..\\output\\";
            appFile.contentroot = sourceFilePath;
            appFile.depots.Clear();
            appFile.depots.Add(depot.DepotID, string.Format("depot_build_{0}.vdf", depot.DepotID));
            appFile.setlive = branch == "none" ? "" : branch;

            string fullPath = GetAppScriptOutputPath(appFile);
            VDFFile.Save(appFile, fullPath);
            yield return null;
        }

        public IEnumerator CreateDepotFiles(DepotVDFFile depot)
        {
            depot.FileExclusion = "*.pdb";
            depot.FileMapping = new DepotFileMapping
            {
                LocalPath = ".\\*",
                DepotPath = ".",
                recursive = true
            };

            string fullPath = GetDepotScriptOutputPath(depot);
            VDFFile.Save(depot, fullPath);
            yield return null;
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

        public IEnumerator Upload(AppVDFFile appFile, Action<bool> callback)
        {
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
                string login = m_uploadProcess.StandardOutput.ReadToEnd();
                Debug.Log(login);
                LogOutSteamResult(login, callback);
                m_uploadProcess.WaitForExit();
                m_uploadProcess.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            yield return null;
        }

        private string CreateSteamArguments(AppVDFFile appFile)
        {
            string fullDirectory = GetAppScriptOutputPath(appFile);
            string arguments = string.Format("+login \"{0}\" \"{1}\" +run_app_build \"{2}\" +quit", UserName,
                UserPassword, fullDirectory);
            return arguments;
        }

        private void LogOutSteamResult(string text, Action<bool> callback)
        {
            int errorTextStartIndex = text.IndexOf("Error!", StringComparison.CurrentCultureIgnoreCase);
            if (errorTextStartIndex >= 0)
            {
                int errorNewLine = text.IndexOf('\n', errorTextStartIndex);
                string errorText = text.Substring(errorTextStartIndex, errorNewLine - errorTextStartIndex);
                Debug.LogError("Failed to log to steam: " + errorText);
                callback(false);
                return;
            }

            string[] lines = text.Split('\n');

            if (!ContainsText(lines, "Loading Steam API", "OK"))
            {
                Debug.LogError("Failed to load API.");
                callback(false);
                return;
            }

            if (!ContainsText(lines, "Logging in user", "OK"))
            {
                Debug.LogError("Failed to log into User account");
                callback(false);
                return;
            }

            if (!ContainsText(lines, "Uploading content...", ""))
            {
                Debug.LogError("Failed to scan content...");
                callback(false);
                return;
            }

            string uploadFailed = "Fail";
            bool uploadingFailed = text.Contains(uploadFailed);
            if (uploadingFailed)
            {
                Debug.LogError("Failed to upload to steam. Check logs for info.");
                callback(false);
                return;
            }

            Debug.Log("Upload successful.");
            callback(true);
        }

        private bool ContainsText(string[] lines, string startsWith, string endsWith)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith(startsWith))
                {
                    if (!string.IsNullOrEmpty(endsWith) && !line.EndsWith(endsWith))
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}