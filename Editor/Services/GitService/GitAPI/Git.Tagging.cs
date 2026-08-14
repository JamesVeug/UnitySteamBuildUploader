using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal static partial class Git
    {
        private const int PushTimeoutMs = 60000;
        private const string InvalidTagCharacters = "@{}/~^:?*[]\\";

        public static bool IsValidTagName(string tag, out string reason)
        {
            if (string.IsNullOrEmpty(tag))
            {
                reason = "it is empty";
                return false;
            }

            foreach (char c in tag)
            {
                if (c == ' ')
                {
                    reason = "it contains a space";
                    return false;
                }

                if (InvalidTagCharacters.IndexOf(c) >= 0)
                {
                    reason = $"it contains '{c}'";
                    return false;
                }
            }

            reason = "";
            return true;
        }

        public static async Task<bool> CreateTag(string tag, UploadTaskReport.StepResult result)
        {
            if (!TryGetExecutable(tag, result, out string git))
            {
                result.AddError($"Couldn't tag commit because couldn't get executable.");
                return false;
            }

            result.AddLog($"git tag {tag}");
            ProcessUtils.ProcessResult process = await Task.Run(() =>
                ProcessUtils.RunSync(git, $"tag {Quote(tag)}", m_projectRoot));

            if (!process.IsSuccessful)
            {
                result.AddError(process.Errors);
                result.AddError($"Failed to create tag '{tag}'.");
                return false;
            }

            // Get a new snapshot so we can confirm the tag was made
            Invalidate();

            result.AddLog($"Tagged {tag}.");
            return true;
        }

        public static async Task<bool> PushTag(string tag, string remote, UploadTaskReport.StepResult result)
        {
            if (!TryGetExecutable(tag, result, out string git))
            {
                return false;
            }

            Dictionary<string, string> environment = new Dictionary<string, string>
            {
                { "GIT_TERMINAL_PROMPT", "0" }
            };

            result.AddLog($"git push {remote} refs/tags/{tag}");
            ProcessUtils.ProcessResult process = await Task.Run(() =>
                ProcessUtils.RunSync(git, $"push {Quote(remote)} refs/tags/{Quote(tag)}", m_projectRoot,
                    PushTimeoutMs, environment));

            if (!process.IsSuccessful)
            {
                result.AddError(process.Errors);
                result.AddError($"Failed to push tag '{tag}' to '{remote}'.");
                return false;
            }

            result.AddLog($"Pushed {tag} to {remote}.");
            return true;
        }

        private static bool TryGetExecutable(string tag, UploadTaskReport.StepResult result, out string git)
        {
            git = Executable;
            if (string.IsNullOrEmpty(git))
            {
                result.AddError($"Cannot tag '{tag}' because no git executable could be found.");
                return false;
            }

            return true;
        }

        private static string Quote(string argument)
        {
            return "\"" + argument + "\"";
        }
    }
}
