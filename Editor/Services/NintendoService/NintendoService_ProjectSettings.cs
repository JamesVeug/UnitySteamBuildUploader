using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class NintendoService
    {
        public override bool HasProjectSettingsGUI => true;

        private NintendoApp _current;
        private Context m_context = new Context();

        private ReorderableListOfNintendoBranches m_branchesList = new ReorderableListOfNintendoBranches();

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                // Current Config
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Title:", GUILayout.Width(100));

                    using (new EditorGUI.DisabledGroupScope(_current == null))
                    {
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            NintendoAppData data = NintendoUIUtils.GetNintendoBuildData();
                            if (_current != null && data.Configs.Contains(_current))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?",
                                        "Are you sure you want to delete the title '" + _current.Name + "'?", "Yes",
                                        "No"))
                                {
                                    data.Configs.Remove(_current);
                                    NintendoUIUtils.Save();
                                    NintendoUIUtils.ConfigPopup.Refresh();
                                    _current = null;
                                }
                            }
                        }
                    }

                    if (NintendoUIUtils.ConfigPopup.DrawPopup(ref _current, m_context))
                    {
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", true, _ => { Save(); });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        NintendoApp config = new NintendoApp();
                        List<NintendoApp> configs = NintendoUIUtils.GetNintendoBuildData().Configs;
                        config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                        configs.Add(config);
                        NintendoUIUtils.Save();
                        NintendoUIUtils.ConfigPopup.Refresh();
                        _current = config;
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", true, _ => { Save(); });
                    }

                    if (_current != null)
                    {
                        if (GUILayout.Button("Developer Portal", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://developer.nintendo.com/");
                        }
                    }
                }

                if (_current == null)
                {
                    return;
                }

                // Draw
                using (new GUILayout.VerticalScope())
                {
                    DrawAppData();
                }

                using (new GUILayout.VerticalScope())
                {
                    DrawBranches();
                }
            }
        }

        public void DrawAppData()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Name:", GUILayout.Width(150));
                string newConfigName = EditorGUILayout.TextField(_current.Name);
                if (newConfigName != _current.Name)
                {
                    _current.Name = newConfigName;
                    Save();
                    NintendoUIUtils.ConfigPopup.Refresh();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Title ID:", GUILayout.Width(150));
                string newTitleId = EditorGUILayout.TextField(_current.TitleID);
                if (newTitleId != _current.TitleID)
                {
                    _current.TitleID = newTitleId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Application ID:", GUILayout.Width(150));
                string newAppId = EditorGUILayout.TextField(_current.ApplicationID);
                if (newAppId != _current.ApplicationID)
                {
                    _current.ApplicationID = newAppId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Branch:", GUILayout.Width(150));
                string newBranch = _current.DefaultBranch;
                var chosenBranch = _current.ConfigBranches.FirstOrDefault(b => b.name == newBranch);
                if (NintendoUIUtils.BranchPopup.DrawPopup(_current, ref chosenBranch, m_context))
                {
                    _current.DefaultBranch = chosenBranch?.name;
                    Save();
                }
            }
        }

        public void DrawBranches()
        {
            if (m_branchesList.OnGUI())
            {
                Save();
                NintendoUIUtils.BranchPopup.Refresh();
            }
        }

        public void Save()
        {
            NintendoUIUtils.Save();
            NintendoUIUtils.BranchPopup.Refresh();
        }
    }
}
