﻿using System.Collections.Generic;
using UnityEditor;

namespace Wireframe
{
    internal class ProjectSettings : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new ProjectSettings("Project/BuildUploader", SettingsScope.Project)
                {
                    label = "Build Uploader",
                    keywords = new HashSet<string>(new[] { "Steam", "Build", "Upload", "Pipe", "line" })
                };

            return provider;
        }

        private ProjectSettings(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            foreach (AService service in InternalUtils.AllServices())
            {
                service.ProjectSettingsGUI();
            }
        }
    }
}