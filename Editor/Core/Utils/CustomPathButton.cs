using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class CustomPathButton
    {
        private static readonly GUIStyle pathButtonDoesNotExistStyle;
        private static readonly GUIStyle pathButtonExistsStyle;

        static CustomPathButton()
        {
            pathButtonExistsStyle = new GUIStyle(GUI.skin.button);
            pathButtonExistsStyle.alignment = TextAnchor.MiddleLeft;
            
            pathButtonDoesNotExistStyle = new GUIStyle(GUI.skin.button);
            pathButtonDoesNotExistStyle.normal.textColor = Color.yellow;
            pathButtonDoesNotExistStyle.alignment = TextAnchor.MiddleLeft;
        }
        
        public static bool OnGUI(ref string fullPath, string emptyPathText, float maxWidth)
        {
            string displayedPath = fullPath;
            if (!string.IsNullOrEmpty(displayedPath))
            {
                float characterWidth = 8f;
                int characters = displayedPath.Length;
                float expectedWidth = characterWidth * characters;
                if (expectedWidth >= maxWidth)
                {
                    int charactersToRemove = (int)((expectedWidth - maxWidth) / characterWidth);
                    if (charactersToRemove < displayedPath.Length)
                    {
                        displayedPath = displayedPath.Substring(charactersToRemove);
                    }
                    else
                    {
                        displayedPath = "";
                    }
                }
                
                if(displayedPath.Length < fullPath.Length)
                {
                    displayedPath = "..." + displayedPath;
                }
            }
            else
            {
                displayedPath = emptyPathText;
            }
            
            GUIStyle style = Utils.PathExists(displayedPath) ? pathButtonExistsStyle : pathButtonDoesNotExistStyle;
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Folder to upload", displayedPath, "");
                fullPath = newPath;
                return true;
            }

            return false;
        }
    }
}