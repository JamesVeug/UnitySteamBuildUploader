using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Wireframe;

public class SteamGuardWindow : EditorWindow
{
    private Action<string> guardCodeCallback;
    private string enteredText;
    private bool waitingForCode;
    

    public static async Task ShowAsync(Action<string> codeCallback)
    {
        var window = GetWindow<SteamGuardWindow>();
        window.enteredText = "";
        window.guardCodeCallback = codeCallback;
        window.waitingForCode = true;
        while (window.waitingForCode)
        {
            await Task.Delay(100);
        }
    }

    private void OnDisable()
    {
        guardCodeCallback = null;
        waitingForCode = false;
    }

    private void OnGUI()
    {
        GUILayout.Label("Looks like you need to verify Steam Guard!\n" +
                        "Enter the Steam Guard code below to continue.");

        enteredText = GUILayout.TextField(enteredText);
        
        if (GUILayout.Button("Retry login"))
        {
            guardCodeCallback?.Invoke(enteredText);
            Close();
        }
    }
}