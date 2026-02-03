using System;
using UnityEngine;

namespace Wireframe
{
    public static class BatchModeUtil
    {
        /// <summary>
        /// Executes upload tasks and uses the command line arguments as potential IDs 
        /// </summary>
        public static void Execute()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; i++)
            {
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