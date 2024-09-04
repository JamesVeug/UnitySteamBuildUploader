using System;
using UnityEditor;
using UnityEngine;
using Wireframe;

public class SteamGuardWindow : EditorWindow
{
    private static string providedText;

    private string text;
    
    public static void Show(string text)
    {
        providedText = text;
        
        var window = GetWindow<SteamGuardWindow>();
        window.text = $"login <steam_username> <steam_password> <steam_guard_code>";
        OpenSteamCMD();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Looks like you need to verify Steam Guard!\n" +
                        "You will need to do this only once so Steam allows access to your Steam account!\n" +
                        "The Steam CMD will automatically open in a few seconds. \n" +
                        "Please fill out the information below and copy+paste it into the console.");

        // Login details
        using (new EditorGUILayout.HorizontalScope())
        {
            text = GUILayout.TextField(text);
            if (GUILayout.Button("Copy To Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = text;
            }
        }
        
        // upload details
        // using (new EditorGUILayout.HorizontalScope())
        // {
        //     text = GUILayout.TextField(text);
        //     if (GUILayout.Button("Reset Text"))
        //     {
        //         text = providedText;
        //     }
        // }

        if(GUILayout.Button("Open Steam CMD"))
        {
            OpenSteamCMD();
        }
    }

    private static void OpenSteamCMD()
    {
        System.Diagnostics.Process.Start(SteamSDK.SteamSDKEXEPath);
    }
}