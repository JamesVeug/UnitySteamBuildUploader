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
            public readonly int ExitCode;
            
            private ProcessResult(bool isSuccessful, string output, string errors, int exitCode)
            {
                IsSuccessful = isSuccessful;
                Output = output;
                Errors = errors;
                ExitCode = exitCode;
            }
            
            public static ProcessResult Successful(string text)
            {
                return new ProcessResult(true, text, "", 0);
            }
            
            public static ProcessResult Failed(string reason)
            {
                return new ProcessResult(false, "", reason, -1);
            }
            
            /// <summary>
            /// The process ran and told us it failed. Keeps whatever it wrote - some tools explain
            /// themselves on stdout and still exit non-zero.
            /// </summary>
            public static ProcessResult Failed(string reason, string output, int exitCode)
            {
                return new ProcessResult(false, output, reason, exitCode);
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

        public static ProcessResult RunSync(string path, string args, string workingDirectory, int timeoutMs = 5000, Dictionary<string, string> environment = null, bool closeStandardInput = false)
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
                    process.StartInfo.RedirectStandardInput = closeStandardInput;

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

                    // Some tools (steamcmd) prompt on stdin when they need input. Closing it makes them
                    // fail immediately with a parsable message instead of blocking until the timeout.
                    if (closeStandardInput)
                    {
                        process.StandardInput.Close();
                    }

                    // Read both streams as they arrive. ReadToEnd on one of them would only return when the
                    // process closes it, which never happens if the process hangs - the timeout below would
                    // never be reached.
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            // Joined with a newline rather than AppendLine - callers parse tool
                            // output that uses '\n' itself.
                            lock (outputBuilder) outputBuilder.Append(e.Data).Append('\n');
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            lock (errorBuilder) errorBuilder.Append(e.Data).Append('\n');
                        }
                    };
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(timeoutMs))
                    {
                        process.Kill();
                        return ProcessResult.Failed($"Timed out after {timeoutMs}ms: {path} {args}");
                    }

                    // Parameterless overload waits for the async readers to drain what is left.
                    process.WaitForExit();

                    string output;
                    string errors;
                    lock (outputBuilder) output = outputBuilder.ToString();
                    lock (errorBuilder) errors = errorBuilder.ToString();

                    if (process.ExitCode != 0)
                    {
                        return ProcessResult.Failed(string.IsNullOrEmpty(errors)
                            ? $"Exited with code {process.ExitCode}: {path} {args}"
                            : errors, output, process.ExitCode);
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