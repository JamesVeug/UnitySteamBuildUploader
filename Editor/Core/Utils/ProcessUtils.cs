using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
        
        public static async Task<ProcessResult> RunTask(UploadTaskReport.StepResult result, string path, string args, params string[] hideText)
        {
#if UNITY_EDITOR_LINUX
            string fileName = "/bin/bash";
            string arguments = "-c \" chmod +x " + path + " " + args;
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

                    if (!process.Start())
                    {
                        string reason = "Could not start process. FileName or arguments are incorrect or the file is busy. Exit: " + process.ExitCode;
                        result.SetFailed(reason);
                        return ProcessResult.Failed(reason);
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    output = HideText(output, hideText);
                    
                    string errors = await process.StandardError.ReadToEndAsync();
                    errors = HideText(errors, hideText);

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
                result.AddException(ex);
                return ProcessResult.Failed(ex.Message);
            }
        }

        private static string HideText(string text, string[] toHide)
        {
            if (toHide == null || toHide.Length == 0)
            {
                return text;
            }

            foreach (string hide in toHide)
            {
                text = text.Replace(hide, "****");
            }
            
            return text;
        }
    }
}