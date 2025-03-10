using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class NewGithubReleaseDestination : ABuildDestination
    {
        public override string DisplayName => "New Github Release";
        
        private string m_owner;
        private string m_repo;
        private string m_releaseName;
        private string m_tagName;
        private string m_target;
        private bool m_draft;
        private bool m_prerelease;
        
        public NewGithubReleaseDestination(BuildUploaderWindow window) : base(window)
        {
        }

        public override async Task<UploadResult> Upload(string filePath, string buildDescription)
        {
            List<string> files = new List<string>();
            files.Add(filePath);

            int processID = ProgressUtils.Start("Github Release", "Uploading to Github Release");
            UploadResult result = await Github.NewRelease(m_owner, m_repo, m_releaseName, buildDescription, m_tagName, m_target, m_draft, m_prerelease, Github.Token, files);
            
            ProgressUtils.Remove(processID);
            return result;
        }

        public override string ProgressTitle()
        {
            return "Uploading to Github Release";
        }

        public override bool IsSetup(out string reason)
        {
            if (!InternalUtils.GetService<GithubService>().IsReadyToStartBuild(out reason))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(Github.Token))
            {
                reason = "Github Token is not set in Preferences";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_owner))
            {
                reason = "Owner is not set";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_repo))
            {
                reason = "Repo is not set";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_releaseName))
            {
                reason = "Release Name is not set";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_tagName))
            {
                reason = "Tag Name is not set";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_target))
            {
                reason = "Target is not set";
                return false;
            }
            
            reason = "";
            return true;
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            string text = $"{m_owner}/{m_repo}/releases/tag/{m_tagName} ({m_target})";
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                Application.OpenURL("https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#create-a-release");
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Owner:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_owner);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Repo:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_repo);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Release Name:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_releaseName);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Tag:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_tagName);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch or Commit:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_target);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Draft:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_draft);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Prerelease:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_prerelease);
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["owner"] = m_owner;
            dict["repo"] = m_repo;
            dict["releaseName"] = m_releaseName;
            dict["tagName"] = m_tagName;
            dict["target"] = m_target;
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            m_owner = s["owner"] as string;
            m_repo = s["repo"] as string;
            m_releaseName = s["releaseName"] as string;
            m_tagName = s["tagName"] as string;
            m_target = s["target"] as string;
        }
    }
}