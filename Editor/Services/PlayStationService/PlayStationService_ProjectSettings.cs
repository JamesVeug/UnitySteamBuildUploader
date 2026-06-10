using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class PlayStationService
    {
        public override bool HasProjectSettingsGUI => true;

        private PlayStationApp _current;
        private Context m_context = new Context();

        private ReorderableListOfPlayStationBranches m_branchesList = new ReorderableListOfPlayStationBranches();

        public override void ProjectSettingsGUI()
        {
            base.ProjectSettingsGUI();
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
                            PlayStationAppData data = PlayStationUIUtils.GetPlayStationBuildData();
                            if (_current != null && data.Configs.Contains(_current))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?",
                                        "Are you sure you want to delete the title '" + _current.Name + "'?", "Yes",
                                        "No"))
                                {
                                    data.Configs.Remove(_current);
                                    PlayStationUIUtils.Save();
                                    PlayStationUIUtils.ConfigPopup.Refresh();
                                    _current = null;
                                }
                            }
                        }
                    }

                    if (PlayStationUIUtils.ConfigPopup.DrawPopup(ref _current, m_context))
                    {
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", true, _ => { Save(); });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        PlayStationApp config = new PlayStationApp();
                        List<PlayStationApp> configs = PlayStationUIUtils.GetPlayStationBuildData().Configs;
                        config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                        configs.Add(config);
                        PlayStationUIUtils.Save();
                        PlayStationUIUtils.ConfigPopup.Refresh();
                        _current = config;
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", true, _ => { Save(); });
                    }

                    if (_current != null)
                    {
                        if (GUILayout.Button("Developer Portal", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://partners.playstation.net/");
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
                    PlayStationUIUtils.ConfigPopup.Refresh();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Title ID:", "Sony-issued title identifier (e.g. PPSA00000_00 / CUSA00000)."), GUILayout.Width(150));
                string newTitleId = EditorGUILayout.TextField(_current.TitleID);
                if (newTitleId != _current.TitleID)
                {
                    _current.TitleID = newTitleId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Content ID:", "Sony Content ID assigned to this title (e.g. EP9000-CUSA00000_00-0000000000000000)."), GUILayout.Width(150));
                string newContentId = EditorGUILayout.TextField(_current.ContentID);
                if (newContentId != _current.ContentID)
                {
                    _current.ContentID = newContentId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Branch:", GUILayout.Width(150));
                string newBranch = _current.DefaultBranch;
                var chosenBranch = _current.ConfigBranches.FirstOrDefault(b => b.name == newBranch);
                if (PlayStationUIUtils.BranchPopup.DrawPopup(_current, ref chosenBranch, m_context))
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
                PlayStationUIUtils.BranchPopup.Refresh();
            }
        }

        public void Save()
        {
            PlayStationUIUtils.Save();
            PlayStationUIUtils.BranchPopup.Refresh();
        }
    }
}
