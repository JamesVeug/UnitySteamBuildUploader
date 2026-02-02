using System;
using UnityEngine;
using Wireframe;

public class BatchModeUtil : MonoBehaviour
{
    public static void Execute()
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int i = 0; i < arguments.Length; i++)
        {
            Debug.Log("Starting task!");

            UploadProfile profile = UploadProfile.FromGUID(arguments[i]);

            if(profile == null)
            {
                Debug.Log($"No profile found with ID : {arguments[i]}");
                continue;
            }

            UploadTask task = new(profile);

            task.StartBlock(true);

            Debug.Log("Started task!");
        }
    }
}
