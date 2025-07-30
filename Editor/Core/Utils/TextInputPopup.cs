using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class TextInputPopup : EditorWindow
    {
        private string inputText = "";
        private bool shouldFocusTextField = true;

        public static void ShowWindow(System.Action<string> onSubmit)
        {
            var window = GetWindow<TextInputPopup>(true, "Enter Text", true);
            window.onSubmit = onSubmit;
            window.titleContent = new GUIContent("Enter Text");
            window.position = new Rect(
                (Screen.currentResolution.width - 300) / 2,
                (Screen.currentResolution.height - 100) / 2,
                300, 100
            );
            window.ShowModalUtility();
        }
        
        void OnEnable()
        {
            Focus();
        }

        private System.Action<string> onSubmit;

        void OnGUI()
        {
            GUILayout.Label("Please enter text:");
            GUI.SetNextControlName("InputText");
            inputText = EditorGUILayout.TextField(inputText);
            if (shouldFocusTextField)
            {
                EditorGUI.FocusTextInControl("InputText");
                shouldFocusTextField = false;
            }

            if (GUILayout.Button("OK"))
            {
                onSubmit?.Invoke(inputText);
                Close();
            }
        }
    }
}