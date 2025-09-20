using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    internal class Itchio
    {
        public static Itchio Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new Itchio();
                }

                return m_instance;
            }
        }
        
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("Itchio_Enabled", false);
            set => ProjectEditorPrefs.SetBool("Itchio_Enabled", value);
        }

        public static string ItchioSDKPath
        {
            get => ProjectEditorPrefs.GetString("Itchio_SDKPath");
            set => ProjectEditorPrefs.SetString("Itchio_SDKPath", value);
        }
        
        public static string ItchioEXEPath
        {
            get => Instance.m_SDKCMDPath;
        }

        public bool IsInitialized => m_initialized;

        private static Itchio m_instance;

        // ItchioCMD fails if you try to run multiple instances of it at the same time.
        // So lock uploading builds to one at a time.
        private static SemaphoreSlim m_lock = new SemaphoreSlim(1);
        
        private Process m_uploadProcess;
        private string m_SDKCMDPath;
        private bool m_initialized;

        private Itchio()
        {
            if (!string.IsNullOrEmpty(ItchioSDKPath))
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            m_initialized = false;
            if (string.IsNullOrEmpty(ItchioSDKPath) || !Directory.Exists(ItchioSDKPath))
            {
                return;
            }

            string exePath = "";
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                exePath = Path.Combine(ItchioSDKPath, "butler.exe");
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                exePath = Path.Combine(ItchioSDKPath, "butler");
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                exePath = Path.Combine(ItchioSDKPath, "butler");
            }
            else
            {
                Debug.LogError("Unsupported platform for Itchio: " + Application.platform);
                return;
            }
            
            if (!File.Exists(exePath))
            {
                Debug.LogError("Could not find bitcher path!");
                return;
            }

            m_SDKCMDPath = exePath;
            m_initialized = true;
        }

        public async Task<bool> Upload(string pathToUpload, string user, string game, List<string> channels, string version, UploadTaskReport.StepResult stepResult)
        {
            await m_lock.WaitAsync();

            try
            {
                bool retry = true;
                while (retry)
                {
                    retry = false;

                    m_uploadProcess = new Process();
                    m_uploadProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    m_uploadProcess.StartInfo.CreateNoWindow = true;
                    m_uploadProcess.StartInfo.UseShellExecute = false;
                    m_uploadProcess.StartInfo.FileName = m_SDKCMDPath;
                    m_uploadProcess.StartInfo.Arguments = CreateUploadBuildItchioArguments(pathToUpload, user, game, version, channels);
                    m_uploadProcess.StartInfo.RedirectStandardError = true;
                    m_uploadProcess.StartInfo.RedirectStandardOutput = true;
                    m_uploadProcess.EnableRaisingEvents = true;

                    try
                    {
                        if (!m_uploadProcess.Start())
                        {
                            stepResult.SetFailed(
                                "Could not start Itchio upload process. Is ItchioCMD installed or busy? Check the path in the preferences.");
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        stepResult.AddException(e);
                        stepResult.SetFailed("Could not start Itchio upload process.\n" + e.Message);
                        return false;
                    }

                    stepResult.AddLog("Uploading to Itchio....");
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    string textDump = await m_uploadProcess.StandardOutput.ReadToEndAsync();
                    stopwatch.Stop();
                    stepResult.AddLog($"Itchio upload took {stopwatch.ElapsedMilliseconds}ms");

                    var outputResults = await LogOutItchioResult(textDump);

                    try
                    {
                        m_uploadProcess.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        // ItchioCMD.exe doesn't like multiple instances of it running at the same time.
                        stepResult.AddException(e);
                    }

                    m_uploadProcess.Close();


                    if (!outputResults.successful)
                    {
                        stepResult.SetFailed("[Itchio] " + outputResults.errorText + "\n\n" + textDump);
                        retry = outputResults.retry;
                    }
                    else
                    {
                        stepResult.AddLog("[Itchio] Itchio upload successful!\n\n" + textDump);
                    }
                }
            }
            catch (Exception e)
            {
                stepResult.AddException(e);
                stepResult.SetFailed("Could not upload to app");
            }
            finally
            {
                m_lock.Release();
            }

            return stepResult.Successful;
        }

        /// <summary>
        /// https://itch.io/docs/butler/pushing.html
        /// </summary>
        private string CreateUploadBuildItchioArguments(string pathToUpload, string user, string game, string version, List<string> channels)
        {
            // push "<pathToUpload>" <user>/<game>:<channel1>-<channel2>-<channel3> --userversion <version>
            string channelArg = string.Join("-", channels.Select(a=>a.ToLower()));
            string arguments = $"push \"{pathToUpload}\" {user}/{game}:{channelArg} --userversion \"{version}\"";

            return arguments;
        }

        
        private readonly string[] failStrings = new string[]
        {
            "missing",
            "error",
            "failed",
            "not found",
            "not recognized",
            "invalid",
            "unauthorized"
        };
        
        private async Task<OutputResultArgs> LogOutItchioResult(string textDump)
        {
            OutputResultArgs result = new OutputResultArgs();
            if (string.IsNullOrEmpty(textDump))
            {
                result.errorText = "Itchio upload failed: No output from ItchioCMD. Does your username/game_id/channel_id have spaces?";
                result.successful = false;
                return result;
            }
            
            foreach (string failString in failStrings)
            {
                int index = textDump.IndexOf(failString, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    int endLineIndex = textDump.IndexOf('\n', index);
                    if (endLineIndex < 0)
                    {
                        endLineIndex = textDump.Length; // If no newline, take the rest of the string
                    }
                    
                    string errorText = textDump.Substring(index, endLineIndex - index);
                    result.errorText = $"Itchio upload failed: {errorText}";
                    result.successful = false;
                    return result;
                }
            }

            result.successful = true;
            return result;
        }

        public void ShowConsole()
        {
            var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(m_SDKCMDPath);  // /k keeps the terminal open, cd /d changes drive if needed
            process.StartInfo.Arguments = $"/k \"{m_SDKCMDPath}\"";  // /k keeps the terminal open, cd /d changes drive if needed
            process.Start();
        }

        private class OutputResultArgs
        {
            public bool successful = false;
            public bool retry = false;
            public string ItchioGuardCode = "";
            public string ItchioTwoFactorCode = "";
            public string errorText;
        }
    }
}