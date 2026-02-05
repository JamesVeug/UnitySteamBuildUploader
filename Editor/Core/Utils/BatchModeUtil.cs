using System;
using UnityEngine;

namespace Wireframe
{
    public static class BatchModeUtil
    {
        /// <summary>
        /// Executes upload tasks and uses the command line arguments to detect GUIDs
        /// Example on how to execute this command with a CLI:
        /// "PATH/TO/UNITY/Unity.exe" -batchmode -quit -projectPath "PATH/TO/PROJECT" -executeMethod Wireframe.BatchModeUtil.Execute -uploadProfile "UPLOAD_PROFILE_GUID"
        /// If the terminal never exits then add -async and the terminal should persist until exiting but complete the task
        /// For more see: https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/Starting-a-BuildTask-without-UI
        /// </summary>
        public static void Execute()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            bool isAsync = false;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Equals("-async", StringComparison.OrdinalIgnoreCase))
                {
                    isAsync = true;
                    break;
                }
            }
            
            bool checkForProfile = false;
            bool startedATask = false;
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

                startedATask = true;
                UploadTask task = new UploadTask(profile);
                task.OnComplete += (report) =>
                {
                    Debug.Log($"Finished task with guid {report.GUID}!");
                };
                
                if (isAsync)
                {
                    Debug.Log($"Starting async task with guid {task.GUID} with Upload Profile GUID: {arguments[i]}!");
                    task.Start();
                }
                else
                {
                    Debug.Log($"Starting task with guid {task.GUID} with Upload Profile GUID: {arguments[i]}!");
                    task.StartAndBlock(true);
                }
            }

            if (!startedATask)
            {
                Debug.LogError($"No tasks were started. Check arguments for -uploadProfile \"GUID\"");
            }
        }
    }
}
