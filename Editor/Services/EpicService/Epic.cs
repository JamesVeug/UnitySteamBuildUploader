using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    internal static partial class Epic
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("epic_enabled", false);
            set => ProjectEditorPrefs.SetBool("epic_enabled", value);
        }

        private static string TokenKey => ProjectEditorPrefs.ProjectID + "EpicSDKPath";
        public static string SDKPath
        {
            get => EditorPrefs.GetString(TokenKey);
            set => EditorPrefs.SetString(TokenKey, value);
        }
    }
}