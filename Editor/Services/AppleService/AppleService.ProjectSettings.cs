using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class AppleService
    {
        private static ReorderableListOfAppleApiKeysProjectSettings _reorderableListOfAppleApiKeysProjectSettings;
        private static ReorderableListOfAppleBetaGroups _reorderableListOfAppleBetaGroups;
        private static AppleConfig.AppleApp m_selectedApp;
        private static Context m_context = new Context();

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                AppleConfig config = AppleUIUtils.GetConfig();

                GUILayout.Label("Apps", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (AppleUIUtils.AppPopup.DrawPopup(ref m_selectedApp, m_context, GUILayout.Width(200)))
                    {
                        _reorderableListOfAppleBetaGroups = null;
                    }

                    if (GUILayout.Button("Add App", GUILayout.Width(100)))
                    {
                        AppleConfig.AppleApp app = new AppleConfig.AppleApp();
                        List<AppleConfig.AppleApp> apps = AppleUIUtils.GetConfig().apps;
                        app.Id = apps.Count > 0 ? apps.Max(a => a.Id) + 1 : 1;
                        apps.Add(app);
                        AppleUIUtils.Save();
                        AppleUIUtils.AppPopup.Refresh();
                        AppleUIUtils.BetaGroupPopup.Refresh();
                        _reorderableListOfAppleBetaGroups = null;
                        m_selectedApp = app;
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledGroupScope(m_selectedApp == null))
                    {
                        if (GUILayout.Button("Remove App", GUILayout.Width(100)))
                        {
                            if (EditorUtility.DisplayDialog("Remove App",
                                    "Are you sure you want to remove the selected Apple app?", "Yes", "No"))
                            {
                                List<AppleConfig.AppleApp> apps = AppleUIUtils.GetConfig().apps;
                                apps.Remove(m_selectedApp);
                                AppleUIUtils.Save();
                                AppleUIUtils.AppPopup.Refresh();
                                AppleUIUtils.BetaGroupPopup.Refresh();
                                m_selectedApp = null;
                            }
                        }
                    }
                }

                using (new GUILayout.VerticalScope("box"))
                {
                    if (m_selectedApp != null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Name:", GUILayout.Width(120));
                            string newName = EditorGUILayout.TextField(m_selectedApp.Name);
                            if (newName != m_selectedApp.Name)
                            {
                                m_selectedApp.Name = newName;
                                AppleUIUtils.Save();
                                AppleUIUtils.AppPopup.Refresh();
                                AppleUIUtils.BetaGroupPopup.Refresh();
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(new GUIContent("Bundle ID:", "The CFBundleIdentifier (e.g. com.example.MyGame)."),
                                GUILayout.Width(120));
                            string newBundle = EditorGUILayout.TextField(m_selectedApp.BundleID);
                            if (newBundle != m_selectedApp.BundleID)
                            {
                                m_selectedApp.BundleID = newBundle;
                                AppleUIUtils.Save();
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(new GUIContent("App Store Connect ID:",
                                    "Numeric \"app\" resource ID from App Store Connect, used for REST API calls. " +
                                    "Find it in the URL when viewing the app in App Store Connect."),
                                GUILayout.Width(120));
                            string newId = EditorGUILayout.TextField(m_selectedApp.AppStoreConnectID);
                            if (newId != m_selectedApp.AppStoreConnectID)
                            {
                                m_selectedApp.AppStoreConnectID = newId;
                                AppleUIUtils.Save();
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Platform:", GUILayout.Width(120));
                            ApplePlatform newPlatform = (ApplePlatform)EditorGUILayout.EnumPopup(m_selectedApp.Platform);
                            if (newPlatform != m_selectedApp.Platform)
                            {
                                m_selectedApp.Platform = newPlatform;
                                AppleUIUtils.Save();
                            }
                        }

                        if (_reorderableListOfAppleBetaGroups == null)
                        {
                            _reorderableListOfAppleBetaGroups = new ReorderableListOfAppleBetaGroups();
                            _reorderableListOfAppleBetaGroups.Initialize(m_selectedApp.betaGroups, "Beta Groups",
                                true, (_) => { AppleUIUtils.Save(); });
                        }

                        if (_reorderableListOfAppleBetaGroups.OnGUI())
                        {
                            AppleUIUtils.Save();
                            AppleUIUtils.BetaGroupPopup.Refresh();
                        }
                    }
                }

                GUILayout.Space(20);

                if (_reorderableListOfAppleApiKeysProjectSettings == null)
                {
                    _reorderableListOfAppleApiKeysProjectSettings = new ReorderableListOfAppleApiKeysProjectSettings();
                    _reorderableListOfAppleApiKeysProjectSettings.Initialize(config.apiKeys, "API Keys",
                        true, (_) => { AppleUIUtils.Save(); });
                }

                GUILayout.Label("API Keys", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("API Keys are created on the App Store Connect dashboard.");
                    if (GUILayout.Button("App Store Connect", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://appstoreconnect.apple.com/access/users");
                    }
                }
                GUILayout.Label("See Edit -> Preferences -> Build Uploader -> Services -> Apple to set the .p8 file path.");

                if (_reorderableListOfAppleApiKeysProjectSettings.OnGUI())
                {
                    AppleUIUtils.Save();
                }
            }
        }
    }
}
