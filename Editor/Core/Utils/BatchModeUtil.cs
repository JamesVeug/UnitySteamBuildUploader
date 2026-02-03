using System;
using UnityEngine;

namespace Wireframe
{
    public static class BatchModeUtil
    {
        /// <summary>
        /// Executes upload tasks and uses the command line arguments to detect IDs
        /// Example on how to execute this command with a CLI:
        /// "PATH/TO/UNITY/Unity.exe" -batchmode -quit -projectPath "PATH/TO/PROJECT" -executeMethod Wireframe.BatchModeUtil.Execute -uploadProfile "UPLOAD_PROFILE_ID"
        /// </summary>
        public static void Execute()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            bool checkForProfile = false;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!checkForProfile && arguments[i].Equals("-uploadProfile", StringComparison.OrdinalIgnoreCase))
                {
                    checkForProfile = true;
                    continue;
                }

                checkForProfile = false;

                UploadProfile profile = UploadProfile.FromGUID(arguments[i]);

                if (profile == null)
                {
                    Debug.Log($"No profile found with ID : {arguments[i]}");
                    continue;
                }

                UploadTask task = new(profile);
                Debug.Log($"Starting task {arguments[i]}!");
                task.StartAndBlock(true);
                Debug.Log($"Finished task {arguments[i]}!");
            }
        }
    }
}