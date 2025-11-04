using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{

    internal static class Process_Utilities
    {
        public static async Task<bool> RunTask(string path, string args)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = path,

                Arguments = args,

                RedirectStandardOutput = true,

                RedirectStandardError = true,

                UseShellExecute = false,

                CreateNoWindow = true
            };

            try
            {
                using Process process = Process.Start(startInfo);

                if (process == null)
                {
                    result.SetFailed("Failed to start process (process is null).");

                    return false;
                }

                string output = await process.StandardOutput.ReadToEndAsync();

                string errors = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                UnityEngine.Debug.Log($"[Process] Output:\n{output}");

                if (!string.IsNullOrEmpty(errors))
                {
                    UnityEngine.Debug.LogError($"[Process] Errors:\n{errors}");
                }

                if (process.ExitCode != 0)
                {
                    result.SetFailed($"Process exited with code {process.ExitCode}.\nSee log for details.");

                    return false;
                }

                return result.Successful;
            }
            catch (Exception ex)
            {
                result.AddException(ex);

                result.SetFailed($"Failed to run Process: {ex.Message}");

                return false;
            }
        }

    }
}