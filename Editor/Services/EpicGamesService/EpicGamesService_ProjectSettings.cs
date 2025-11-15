using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EpicGamesService
    {
        private ReorderableListOfArtifacts m_artifactList = new ReorderableListOfArtifacts();
        private StringFormatter.Context m_context = new StringFormatter.Context();
        private EpicGamesOrganization selectedOrganization;
        private EpicGamesProduct selectedGame;
        
        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                // Organization
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Organization:", GUILayout.Width(100));

                    using (new EditorGUI.DisabledGroupScope(selectedOrganization == null))
                    {
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            EpicGamesAppData data = EpicGamesUIUtils.GetEpicGamesData();
                            if (selectedOrganization != null && data.Organizations.Contains(selectedOrganization))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?",
                                        "Are you sure you want to delete the Organization '" +
                                        selectedOrganization.Name + "'?", "Yes",
                                        "No"))
                                {
                                    data.Organizations.Remove(selectedOrganization);
                                    Save();
                                    selectedOrganization = null;
                                    selectedGame = null;
                                }
                            }
                        }
                    }
                    
                    if (EpicGamesUIUtils.OrganizationPopup.DrawPopup(ref selectedOrganization, m_context))
                    {
                        selectedGame = null;
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        EpicGamesOrganization config = new EpicGamesOrganization();
                        List<EpicGamesOrganization> organizations = EpicGamesUIUtils.GetEpicGamesData().Organizations;
                        config.ID = organizations.Count > 0 ? organizations[organizations.Count - 1].Id + 1 : 1;
                        organizations.Add(config);
                        Save();
                        selectedOrganization = config;
                        selectedGame = null;
                    }
                }

                if (selectedOrganization == null)
                {
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Name:", GUILayout.Width(100));
                    if (CustomTextField.Draw(ref selectedOrganization.Name))
                    {
                        Save();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Org ID:", GUILayout.Width(100));
                        
                    string productID = PasswordField.Draw("", "SECRET!", 0, selectedOrganization.OrganizationID);
                    if (productID != selectedOrganization.OrganizationID)
                    {
                        selectedOrganization.OrganizationID = productID;
                        Save();
                    }
                }

                // Game
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Product:", GUILayout.Width(100));

                    using (new EditorGUI.DisabledGroupScope(selectedOrganization == null))
                    {
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            List<EpicGamesProduct> games = selectedOrganization.Products;
                            if (selectedGame != null && games.Contains(selectedGame))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?",
                                        "Are you sure you want to delete the Game '" +
                                        selectedGame.Name + "'?", "Yes",
                                        "No"))
                                {
                                    games.Remove(selectedGame);
                                    Save();
                                    selectedGame = null;
                                }
                            }
                        }
                    }
                    
                    if (EpicGamesUIUtils.ProductPopup.DrawPopup(selectedOrganization, ref selectedGame, m_context))
                    {
                        m_artifactList.Initialize(selectedGame.Artifacts, "Artifacts", true, _ => { Save(); });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        EpicGamesProduct config = new EpicGamesProduct();
                        List<EpicGamesProduct> games = selectedOrganization.Products;
                        config.ID = games.Count > 0 ? games.Max(a=>a.Id) + 1 : 1;
                        games.Add(config);
                        Save();
                        selectedGame = config;
                        m_artifactList.Initialize(selectedGame.Artifacts, "Artifacts", true, _ => { Save(); });
                    }
                }

                if (selectedGame == null)
                {
                    return;
                }

                GUIStyle indent= "";
                indent.margin.left = 20;
                using (new EditorGUILayout.VerticalScope(indent))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Name:", GUILayout.Width(100));
                        if (CustomTextField.Draw(ref selectedGame.Name))
                        {
                            Save();
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Product ID:", GUILayout.Width(100));
                        
                        string productID = PasswordField.Draw("", "", 0, selectedGame.ProductID);
                        if (productID != selectedGame.ProductID)
                        {
                            selectedGame.ProductID = productID;
                            Save();
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Client ID:", GUILayout.Width(100));
                        string clientID = PasswordField.Draw("", "", 0, selectedGame.ClientID);
                        if (clientID != selectedGame.ClientID)
                        {
                            selectedGame.ClientID = clientID;
                            Save();
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    { 
                        GUIContent content = new GUIContent();
                        content.text = "Secret:";
                        content.tooltip = "The secret required to authenticate with Epic Games services." +
                                          "\n\nEnv Var: The secret is stored in an environment variable. Useful for CI/CD setups. Synced via Version Control." +
                                          "\n\nClient Secret: Direct client Secret. Useful for local setups. Not synced via Version Control and encrypted.";
                        EditorGUILayout.LabelField(content, GUILayout.Width(100));
                        
                        if (GUILayout.Button("?", GUILayout.Width(20)))
                        {
                            Application.OpenURL("https://dev.epicgames.com/docs/epic-games-store/publishing-tools/uploading-binaries/bpt-instructions-170#retrieve-credential-ids");
                        }
                        
                        var newHandler = (EpicGamesProduct.SecretTypes)EditorGUILayout.EnumPopup(selectedGame.SecretType, GUILayout.Width(100));
                        if (newHandler != selectedGame.SecretType)
                        {
                            selectedGame.SecretType = newHandler;
                            Save();
                        }


                        switch (newHandler)
                        {
                            case EpicGamesProduct.SecretTypes.EnvVar:
                                if (CustomTextField.Draw(ref selectedGame.ClientSecretEnvVar))
                                {
                                    Save();
                                }
                                break;
                            case EpicGamesProduct.SecretTypes.ClientSecret:
                                string password = GetClientSecret(selectedOrganization.OrganizationID, selectedGame.ClientID);
                                string newPassword = PasswordField.Draw("", "", 0, password);
                                if (password != newPassword)
                                {
                                    SetClientSecret(selectedOrganization.OrganizationID, selectedGame.ClientID, newPassword);
                                }
                                break;
                            default:
                                EditorGUILayout.LabelField("Unknown secret type: " + newHandler);
                                break;
                        }
                    }

                    

                    if(m_artifactList.OnGUI())
                    {
                        Save();
                    }
                }
            }
        }

        private static void Save()
        {
            EpicGamesUIUtils.Save();
            EpicGamesUIUtils.RefreshAllPopups();
        }
    }
}