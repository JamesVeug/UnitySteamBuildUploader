using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    [BuildDestination("New Github Release")]
    public partial class NewGithubReleaseDestination : ABuildDestination
    {
        private string m_owner;
        private string m_repo;
        private string m_releaseName;
        private string m_tagName;
        private string m_target;
        private bool m_draft;
        private bool m_prerelease;
        private bool m_zipContents;

        public NewGithubReleaseDestination() : base()
        {
            // Required for reflection
        }
        
        public void SetRepository(string owner, string repo)
        {
            m_owner = owner;
            m_repo = repo;
        }
        
        public void SetRelease(string releaseName, string tagName, string target)
        {
            m_releaseName = releaseName;
            m_tagName = tagName;
            m_target = target;
        }
        
        public void SetContent(bool draft, bool prerelease, bool zipContents)
        {
            m_draft = draft;
            m_prerelease = prerelease;
            m_zipContents = zipContents;
        }

        public override async Task<bool> Upload(BuildTaskReport.StepResult result)
        {
            List<string> files = new List<string>();
            if (m_zipContents)
            {
                // Before uploading the directory we'll zip all its contents
                files.Add(m_filePath);
            }
            else
            {
                // Get all files at the top level so each can be uploaded individually
                // Sub-Folders will be zipped 
                files.AddRange(Directory.GetFiles(m_filePath, "*.*", SearchOption.TopDirectoryOnly));
            }

            int processID = ProgressUtils.Start("Github Release", ProgressTitle());
            
            bool success = await Github.NewRelease(m_owner, m_repo, m_releaseName, m_buildDescription, m_tagName, m_target, m_draft, m_prerelease, Github.Token, result, files);
            
            ProgressUtils.Remove(processID);
            return success;
        }

        public override string ProgressTitle()
        {
            return "Uploading a new Github Release";
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

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["owner"] = m_owner;
            dict["repo"] = m_repo;
            dict["releaseName"] = m_releaseName;
            dict["tagName"] = m_tagName;
            dict["target"] = m_target;
            dict["zipContents"] = m_zipContents;
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            m_owner = s["owner"] as string;
            m_repo = s["repo"] as string;
            m_releaseName = s["releaseName"] as string;
            m_tagName = s["tagName"] as string;
            m_target = s["target"] as string;
            m_zipContents = (bool) s["zipContents"];
        }
    }
}