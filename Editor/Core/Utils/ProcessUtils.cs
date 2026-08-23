using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    public static class ProcessUtils
    {
        public readonly struct ProcessResult
        {
            public readonly bool IsSuccessful;
            public readonly string Output;
            public readonly string Errors;
            
            private ProcessResult(bool isSuccessful, string output, string errors)
            {
                IsSuccessful = isSuccessful;
                Output = output;
                Errors = errors;
            }
            
            public static ProcessResult Successful(string text)
            {
                return new ProcessResult(true, text, "");
            }
            
            public static ProcessResult Failed(string reason)
            {
                return new ProcessResult(false, "", reason);
            }
        }
        
        public static async Task<ProcessResult> RunTask(UploadTaskReport.StepResult result, string path, string args, Dictionary<string,string> environment, params string[] hideText)
        {
#if UNITY_EDITOR_LINUX
            string fileName = "/bin/bash";
            string arguments = $"-c \" chmod +x {path} {args}";
#else
            string fileName = path;
            string arguments = args;
#endif
        
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.EnableRaisingEvents = true;
                
                    if (environment != null)
                    {
                        foreach (var keyValuePair in environment)
                        {
                            process.StartInfo.EnvironmentVariables.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }
            
                    if (!process.Start())
                    {
                        string reason = "Could not start process. FileName or arguments are incorrect or the file is busy. Exit: " + process.ExitCode;
                        result.SetFailed(reason);
                        return ProcessResult.Failed(reason);
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    Debug.Log(output);

                    string errors = await process.StandardError.ReadToEndAsync();
                    Debug.LogError(errors);
                
                    process.WaitForExit();
                    
                    result.AddLog(output);
                    if (!string.IsNullOrEmpty(errors))
                    {
                        result.AddError(errors);
                    }

                    return ProcessResult.Successful(output);
                }
            }
            catch (Exception ex)
            {
                result.AddException(ex, hideText);
                return ProcessResult.Failed(ex.Message.HideText(hideText));
            }
        }

        public static ProcessResult RunSync(string path, string args, string workingDirectory, int timeoutMs = 5000, Dictionary<string, string> environment = null)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = path;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.WorkingDirectory = workingDirectory ?? "";
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    if (environment != null)
                    {
                        foreach (KeyValuePair<string, string> variable in environment)
                        {
                            process.StartInfo.EnvironmentVariables[variable.Key] = variable.Value;
                        }
                    }

                    if (!process.Start())
                    {
                        return ProcessResult.Failed("Could not start process: " + path);
                    }

                    // Only used for tiny outputs, so draining one stream then the other can't fill the
                    // other's buffer and deadlock.
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();

                    if (!process.WaitForExit(timeoutMs))
                    {
                        process.Kill();
                        return ProcessResult.Failed($"Timed out after {timeoutMs}ms: {path} {args}");
                    }

                    if (process.ExitCode != 0)
                    {
                        return ProcessResult.Failed(string.IsNullOrEmpty(errors)
                            ? $"Exited with code {process.ExitCode}: {path} {args}"
                            : errors);
                    }

                    return ProcessResult.Successful(output);
                }
            }
            catch (Exception ex)
            {
                return ProcessResult.Failed(ex.Message);
            }
        }
    }
}