using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Auto picks the location of the last build made from Unity.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(LastBuildSource), "sources", "Chooses the directory of the last build made using the Build Uploader")]
    [UploadSource("LastBuild", "Last Build Directory", false)]
    public partial class LastBuildSource : AUploadSource
    {
        public override async Task<bool> GetSource(bool doNotCache, UploadConfig uploadConfig,
            UploadTaskReport.StepResult stepResult,
            CancellationTokenSource token)
        {
            // Wait for our turn if we need to
            await BuildUtils.WaitForTurnToBuild();

            try
            {
                if (string.IsNullOrEmpty(LastBuildUtil.LastBuildDirectory))
                {
                    stepResult.AddError("No last build directory found. Please build your project first.");
                    return false;
                }

                if (!Directory.Exists(LastBuildUtil.LastBuildDirectory))
                {
                    stepResult.AddError($"Last build directory does not exist: {LastBuildUtil.LastBuildDirectory}");
                    return false;
                }
            }
            finally
            {
                BuildUtils.ReleaseBuildLock();
            }

            return true;
        }

        public override string SourceFilePath()
        {
            return LastBuildUtil.LastBuildDirectory;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (string.IsNullOrEmpty(LastBuildUtil.LastBuildDirectory))
            {
                errors.Add("No last build directory found. Please build your project first.");
            }
            else if (!Directory.Exists(LastBuildUtil.LastBuildDirectory))
            {
                errors.Add($"Last build directory does not exist: {LastBuildUtil.LastBuildDirectory}");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object> { };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            
        }
    }
}