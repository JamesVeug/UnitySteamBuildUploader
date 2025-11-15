using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Wireframe
{
    public static class ProcessUtils
    {
        public readonly struct ProcessResult
        {
            public readonly bool Successful;
            public readonly string Output;
            public readonly string Errors;
            
            public ProcessResult(bool successful, string output, string errors)
            {
                Successful = successful;
                Output = output;
                Errors = errors;
            }
        }
        
        public static async Task<ProcessResult> RunTask(UploadTaskReport.StepResult result,string path, string args)
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
                    result.AddError("Failed to start process (process is null).");
                    return new ProcessResult(false, "", "Failed to start process (process is null).");
                }

                string output = await process.StandardOutput.ReadToEndAsync();

                string errors = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                result.AddLog(output);

                if (!string.IsNullOrEmpty(errors))
                {
                    result.AddError(errors);
                }

                return new ProcessResult(process.ExitCode == 0, output, errors);
            }
            catch (Exception ex)
            {
                result.AddException(ex);
                return new ProcessResult(false, "", ex.Message);
            }
        }

    }
}