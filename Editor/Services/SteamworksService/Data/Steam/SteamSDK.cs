using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEditor;
using UnityEngine.Networking;

namespace Wireframe
{
    internal class SteamSDK
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

        static SteamSDK()
        {
            // V2.1 - Migrate old preferences to new encoded values
            EncodedEditorPrefs.MigrateKeyToEncoded<string>("steambuild_SDKUser", UserNameKey);
            EncodedEditorPrefs.MigrateKeyToEncoded<string>("steambuild_SDKPass", UserPasswordKey);
        }
        
        public static bool Enabled
        {
            get => EditorPrefs.GetBool("steambuild_Enabled", true);
            set => EditorPrefs.SetBool("steambuild_Enabled", value);
        }

        public static string SteamSDKPath
        {
            get => PlayerPrefs.GetString("steambuild_SDKPath");
            set => PlayerPrefs.SetString("steambuild_SDKPath", value);
        }

        
        private static string UserNameKey => Application.productName + "SteamUBuildUploader";
        public static string UserName
        {
            get => EncodedEditorPrefs.GetString(UserNameKey, "");
            set => EncodedEditorPrefs.SetString(UserNameKey, value);
        }

        private static string UserPasswordKey => Application.productName + "SteamPBuildUploader";
        public static string UserPassword
        {
            get => EncodedEditorPrefs.GetString(UserPasswordKey, "");
            set => EncodedEditorPrefs.SetString(UserPasswordKey, value);
        }
        
        public static string SteamSDKEXEPath
        {
            get => Instance.m_steamCMDPath;
        }

        public bool IsInitialized => m_initialized;

        private static SteamSDK m_instance;

        private Process m_uploadProcess;
        private string m_scriptPath;
        private string m_contentPath;
        private string m_steamCMDPath;
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

            Debug.Log("[SteamSDK] Application.platform: " + Application.platform);
            
            string exePath = "";
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                exePath = Path.Combine(contentBuilderPath, "builder", "steamcmd.exe");
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                exePath = Path.Combine(contentBuilderPath, "builder_osx", "steamcmd.sh");
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                exePath = Path.Combine(contentBuilderPath, "builder_linux", "steamcmd.sh");
            }
            else
            {
                Debug.LogError("Unsupported platform for Steamworks SDK: " + Application.platform);
                return;
            }
            
            
            if (!File.Exists(exePath))
            {
                Debug.LogError("Could not find steamcmd.exe inside ContentBuilder/builder path!");
                return;
            }

            m_steamCMDPath = exePath;
            m_contentPath = content;
            m_scriptPath = scripts;
            m_initialized = true;
        }

        public async Task<bool> CreateAppFiles(AppVDFFile appFile, DepotVDFFile depot, string branch,
            string description,
            string sourceFilePath, BuildTaskReport.StepResult result)
        {
            appFile.desc = description;
            appFile.buildoutput = "..\\output\\";
            appFile.contentroot = sourceFilePath;
            appFile.depots.Clear();
            appFile.depots.Add(depot.DepotID, string.Format("depot_build_{0}.vdf", depot.DepotID));
            appFile.setlive = branch == "none" ? "" : branch;

            string fullPath = GetAppScriptOutputPath(appFile);
            return await VDFFile.Save(appFile, fullPath, result);
        }

        public async Task<bool> CreateDepotFiles(DepotVDFFile depot, BuildTaskReport.StepResult result)
        {
            depot.FileExclusion = "*.pdb";
            depot.FileMapping = new DepotFileMapping
            {
                LocalPath = ".\\*",
                DepotPath = ".",
                recursive = true
            };

            string fullPath = GetDepotScriptOutputPath(depot);
            return await VDFFile.Save(depot, fullPath, result);
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

        public async Task<bool> Upload(AppVDFFile appFile, bool uploadeToSteam, BuildTaskReport.StepResult stepResult)
        {
            try
            {
                bool retry = true;
                string steamGuardCode = "";
                while (retry)
                {
                    retry = false;
                    
                    m_uploadProcess = new Process();
                    m_uploadProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    m_uploadProcess.StartInfo.CreateNoWindow = true;
                    m_uploadProcess.StartInfo.UseShellExecute = false;
                    m_uploadProcess.StartInfo.FileName = m_steamCMDPath;
                    m_uploadProcess.StartInfo.Arguments = CreateUploadBuildSteamArguments(appFile, true, uploadeToSteam, steamGuardCode);
                    m_uploadProcess.StartInfo.RedirectStandardError = true;
                    m_uploadProcess.StartInfo.RedirectStandardOutput = true;
                    m_uploadProcess.EnableRaisingEvents = true;
                    m_uploadProcess.Start();
                    
                    string textDump = await m_uploadProcess.StandardOutput.ReadToEndAsync();
                    
                    // Hide username
                    if (UserName != null && UserName.Length > 2)
                    {
                        textDump = textDump.Replace(UserName, "**********");
                    }
                    
                    var outputResults = await LogOutSteamResult(textDump, uploadeToSteam, false);
                    m_uploadProcess.WaitForExit();
                    m_uploadProcess.Close();

                    if (!outputResults.successful)
                    {
                        stepResult.AddError("[Steam] " + outputResults.errorText + "\n\n" + textDump);
                        retry = outputResults.retry;
                        if (!string.IsNullOrEmpty(outputResults.steamGuardCode))
                        {
                            steamGuardCode = outputResults.steamGuardCode;
                        }
                        if (!string.IsNullOrEmpty(outputResults.steamTwoFactorCode))
                        {
                            steamGuardCode = outputResults.steamTwoFactorCode;
                        }
                    }
                    else
                    {
                        stepResult.AddLog("[Steam] Steam upload successful!\n\n" + textDump);
                    }
                }
            }
            catch (Exception e)
            {
                stepResult.AddException(e);
                stepResult.SetFailed("Could not upload to " + appFile.appid + "\n" + e.Message);
            }

            await Task.Delay(10);
            return stepResult.Successful;
        }

        private string CreateUploadBuildSteamArguments(AppVDFFile appFile, bool quitOnComplete, bool upload, string steamGuardCode)
        {
            string fullDirectory = GetAppScriptOutputPath(appFile);
            
            string username = UserName;
            string password = UserPassword;
            string guard = string.IsNullOrEmpty(steamGuardCode) ? "" : " " + steamGuardCode;
            if (!upload)
            {
                Debug.Log("Upload to Steam is disabled. Not but still attempting login.");
            }
            
            string uploadArg = upload ? $" +run_app_build \"{fullDirectory}\"" : "";
            string arguments = string.Format("+login \"{0}\" \"{1}\" {2} {3}", username, password, guard, uploadArg);
            
            if (quitOnComplete)
            {
                arguments += " +quit";
            }
            return arguments;
        }
        
        private string CreateDRMWrapSteamArguments(bool quitOnComplete, bool upload, string steamGuardCode, int appID, string sourcePath, string destinationPath, int flags)
        {
            string username = UserName;
            string password = UserPassword;
            string guard = string.IsNullOrEmpty(steamGuardCode) ? "" : " " + steamGuardCode;
            if (!upload)
            {
                Debug.Log("Upload to Steam is disabled. Not but still attempting login.");
            }
            
            string uploadArg = !upload ? "" : $" +drm_wrap {appID} \"{sourcePath}\" \"{destinationPath}\" drmtoolp {flags}";
            string arguments = $"+login \"{username}\" \"{password}\" {guard} {uploadArg}";
            
            if (quitOnComplete)
            {
                arguments += " +quit";
            }
            return arguments;
        }

        private class OutputResultArgs
        {
            public bool successful = false;
            public bool retry = false;
            public string steamGuardCode = "";
            public string steamTwoFactorCode = "";
            public string errorText;
        }
        
        private async Task<OutputResultArgs> LogOutSteamResult(string text, bool uploading, bool drmWrapping)
        {
            OutputResultArgs result = new OutputResultArgs();
            
            int errorTextStartIndex = text.IndexOf("Error!", StringComparison.CurrentCultureIgnoreCase);
            if (errorTextStartIndex >= 0)
            {
                int errorNewLine = text.IndexOf('\n', errorTextStartIndex);
                result.errorText = text.Substring(errorTextStartIndex, errorNewLine - errorTextStartIndex);
                return result;
            }

            string[] lines = text.Split('\n');
            int index = -1;
            
            if (!ContainsText(lines, "Loading Steam API", "OK", out index))
            {
                result.errorText = "Failed to load API.";
                return result;
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
                    result.errorText = "Failed to log into User account: " + context;
                }
                else
                {
                    result.errorText = "Failed to log into User account: ";
                }
 
                return result;
            }
            
            if (ContainsText(lines, "Steam Guard code:FAILED", "", out index))
            {
                await SteamGuardWindow.ShowAsync((code) =>
                {
                    result.steamGuardCode = code;
                    result.retry = !string.IsNullOrEmpty(code);
                    if (result.retry)
                    {
                        result.errorText = "Retrying Steam Guard with code";
                    }
                    else
                    {
                        result.errorText = "Steam Guard code was not entered. Cannot continue.";
                    }
                });

                return result;
            }
            
            if (ContainsText(lines, "Two-factor code:FAILED", "", out index) ||
                ContainsText(lines, "Wait for confirmation timed out", "", out index))
            {
                await SteamGuardTwoFactorWindow.ShowAsync((confirmed) =>
                {
                    result.retry = confirmed;
                    if (result.retry)
                    {
                        result.errorText = "Retrying Steam Guard two factor after confirmation on device";
                    }
                    else
                    {
                        result.errorText = "Steam Guard code rejected.";
                    }
                },
                (twoFactorCode) =>
                {
                    result.steamTwoFactorCode = twoFactorCode;
                    result.retry = !string.IsNullOrEmpty(twoFactorCode);
                    if (result.retry)
                    {
                        result.errorText = "Retrying with entered Steam Guard two factor code";
                    }
                    else
                    {
                        result.errorText = "Steam Guard two factor code was not entered. Cannot continue.";
                    }
                });

                return result;
            }

            if (text.Contains("Rate Limit Exceeded"))
            {
                result.errorText = "You tried logging in too many times. Try again later.";
                return result;
            }

            if (text.Contains("Invalid Password"))
            {
                result.errorText = "Incorrect username or password.";
                return result;
            }

            if (uploading)
            {
                if (drmWrapping)
                {
                    if (!ContainsText(lines, "Module is already DRM protected", "", out index)) // TODO: Separate this into a separate verification check
                    {
                        if (!ContainsText(lines, "DRM wrap completed", "", out index))
                        {
                            result.errorText = "Failed to DRM wrap...";
                            return result;
                        }
                    }
                }
                else
                {
                    if (!ContainsText(lines, "Uploading content...", "", out index))
                    {
                        result.errorText = "Failed to scan content to upload...";
                        return result;
                    }
                }
            }
            
            if (ContainsText(lines, "ERROR! Failed to commit build", "", out index))
            {
                result.errorText = "The build may have uploaded but the settings your provided were incorrect. Check Steamworks if the build is there.\n" +
                                   "Possible reasons: Your branch does not exist on Steamworks.";
                return result;
            }
            
            if (text.Contains("Fail") || text.Contains("FAILED"))
            {
                result.errorText = "Failed to upload to steam. Check logs for info!";
                return result;
            }

            result.successful = true;
            return result;
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
            process.StartInfo.FileName = m_steamCMDPath;
            process.StartInfo.Arguments = "";
            process.EnableRaisingEvents = true;
            process.Start();
        }
        
        /// <summary>
        /// The Steam DRM wrapper protects against extremely casual piracy (i.e. copying all game files to another computer) and has some obfuscation, but it is easily removed by a motivated attacker.
        /// https://partner.steamgames.com/doc/features/drm#other_drm
        ///
        /// Works by uploading the .exe of your game and applies the DRM then writes it back to the output path.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DRMWrap(int appID, string sourceExe, string resultEXE, int flags, BuildTaskReport.StepResult stepResult)
        {
            // Get name of sourceExe
            string sourceExeName = Path.GetFileName(sourceExe);
            stepResult.AddLog($"[Steam] Attempting DRMWrap {sourceExeName}. If you're using Steam Guard or Two-factor Authenticator check your phone!\n\n");
            try
            {
                bool retry = true;
                string steamGuardCode = "";
                while (retry)
                {
                    retry = false;
                    
                    m_uploadProcess = new Process();
                    m_uploadProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    m_uploadProcess.StartInfo.CreateNoWindow = true;
                    m_uploadProcess.StartInfo.UseShellExecute = false;
                    m_uploadProcess.StartInfo.FileName = m_steamCMDPath;
                    m_uploadProcess.StartInfo.Arguments = CreateDRMWrapSteamArguments(true, true, steamGuardCode, appID, sourceExe, resultEXE, flags);
                    m_uploadProcess.StartInfo.RedirectStandardError = true;
                    m_uploadProcess.StartInfo.RedirectStandardOutput = true;
                    m_uploadProcess.EnableRaisingEvents = true;
                    m_uploadProcess.Start();
                    
                    string textDump = await m_uploadProcess.StandardOutput.ReadToEndAsync();
                    
                    // Hide username
                    if (UserName != null && UserName.Length > 2)
                    {
                        textDump = textDump.Replace(UserName, "**********");
                    }
                    
                    var outputResults = await LogOutSteamResult(textDump, true, true);
                    m_uploadProcess.WaitForExit();
                    m_uploadProcess.Close();

                    if (!outputResults.successful)
                    {
                        stepResult.AddError("[Steam] " + outputResults.errorText + "\n\n" + textDump);
                        retry = outputResults.retry;
                        if (!string.IsNullOrEmpty(outputResults.steamGuardCode))
                        {
                            steamGuardCode = outputResults.steamGuardCode;
                        }

                        if (!string.IsNullOrEmpty(outputResults.steamTwoFactorCode))
                        {
                            steamGuardCode = outputResults.steamTwoFactorCode;
                        }

                        if (!retry)
                        {
                            stepResult.SetFailed(stepResult.Logs[^1].Message);
                        }
                    }
                    else
                    {
                        stepResult.AddLog("[Steam] Steam DRMWrap successful!\n\n" + textDump);
                    }
                }
            }
            catch (Exception e)
            {
                stepResult.AddException(e);
                stepResult.SetFailed(e.Message);
            }

            return stepResult.Successful;
        }

        /// <summary>
        /// Looks for Readme.txt in Steam SDK path and tries to find the version number.
        /// The version number is listed in the Readme like so:
        /// ----------------------------------------------------------------
        /// v1.59 9th February 2024
        /// ----------------------------------------------------------------
        /// </summary>
        /// <param name="version">Version we currently have otherwise an error message why we couldn't get it.</param>
        /// <returns>True if we retrievwed it</returns>
        public static bool TryCurrentVersion(out string version)
        {
            // Find Readme.txt in SteamSDKPath
            FileInfo[] files = new DirectoryInfo(SteamSDKPath).GetFiles("Readme.txt", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                version = $"Could not find Readme.txt in Steam SDK path. ({SteamSDKPath})";
                return false;
            }

            foreach (FileInfo file in files)
            {
                // Read and check for "Welcome to the Steamworks SDK" line
                string[] lines = File.ReadAllLines(file.FullName);
                foreach (string line in lines)
                {
                    if (line.Contains("Welcome to the Steamworks SDK"))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            var line2 = lines[i];
                            if (line2.StartsWith("----------------------------------------------------------------"))
                            {
                                version = lines[i + 1].Split(' ')[0];
                                return true ;
                            }
                        }

                        version = "Found the right Readme.mexe but unable to find version in Readme.txt";
                        return false;
                    }
                }
            }
            
            version = "Could not find a Readme.txt that has \"Welcome to the Steamworks SDK\" in it.";
            return false;
        }

        [Obsolete("Does not work. Takes you to the steam partners page to login. I don't know how to handle this properly but its a paint to write so leaving it here.")]
        private static async Task<Tuple<bool, string>> TryGetLatestOnlineVersion()
        {
            Debug.Log("Getting latest version from Steam SDK website...");
            string url = "https://partner.steamgames.com/downloads/list";
            UnityWebRequest html = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation operation = html.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Delay(10);
            }
            
            if (html.isNetworkError || html.isHttpError)
            {
                return new Tuple<bool, string>(false, $"Failed to get latest version from {url}. Error: {html.error}");
            }
            
            string text = html.downloadHandler.text;
            Debug.Log(text);
            return new Tuple<bool, string>(true, text);
        }
    }
}