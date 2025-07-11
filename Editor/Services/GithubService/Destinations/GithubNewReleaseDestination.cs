using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("GithubNewRelease", "destinations", "Create a new release on a specific Github repository.")]
    [BuildDestination("GithubNewRelease")]
    public partial class GithubNewReleaseDestination : ABuildDestination
    {
        [Wiki("Owner", "The owner of the repository.")]
        private string m_owner;
        
        [Wiki("Repo", "The name of the repository.")]
        private string m_repo;
        
        [Wiki("Release Name", "The name of the release that appears on Github.")]
        private string m_releaseName;
        
        [Wiki("Tag Name", "The tag that is attached to this release. eg: v1.0.0")]
        private string m_tagName;
        
        [Wiki("Target", "Branch name or commit hash to attach to the release. eg: main")]
        private string m_target;
        
        [Wiki("Draft", "If true, the release will not be published but be editable on github.")]
        private bool m_draft = true;
        
        [Wiki("Prerelease", "If true, marks the release as pre-release.")]
        private bool m_prerelease = true;
        
        [Wiki("ZipContents", "Each file is uploaded individually to Github as a separate download. If true, all files will be sent as a single compressed file instead.")]
        private bool m_zipContents = true;

        public GithubNewReleaseDestination() : base()
        {
            // Required for reflection
        }
        
        public GithubNewReleaseDestination(string owner, string repo, string releaseName, string tagName, string target, bool draft=true, bool prerelease=true, bool zipContents=true) : base()
        {
            SetRepository(owner, repo);
            SetRelease(releaseName, tagName, target);
            SetContent(draft, prerelease, zipContents);
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
            string filePath = StringFormatter.FormatString(m_filePath);
            
            List<string> files = new List<string>();
            if (m_zipContents)
            {
                // Before uploading the directory we'll zip all its contents
                files.Add(filePath);
            }
            else
            {
                // Get all files at the top level so each can be uploaded individually
                // Sub-Folders will be zipped 
                files.AddRange(Directory.GetFiles(filePath, "*.*", SearchOption.TopDirectoryOnly));
            }


            string owner = StringFormatter.FormatString(m_owner);
            string repo = StringFormatter.FormatString(m_repo);
            string releaseName = StringFormatter.FormatString(m_releaseName);
            string tagName = StringFormatter.FormatString(m_tagName);
            string target = StringFormatter.FormatString(m_target);
            
            int processID = ProgressUtils.Start("Github Release", "Uploading a new Github Release");
            bool success = await Github.NewRelease(owner, repo, releaseName, m_buildDescription, tagName, target, m_draft, m_prerelease, Github.Token, result, files);
            ProgressUtils.Remove(processID);
            
            return success;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (!InternalUtils.GetService<GithubService>().IsReadyToStartBuild(out string reason))
            {
                errors.Add(reason);
            }
            
            if (string.IsNullOrEmpty(Github.Token))
            {
                errors.Add("Github Token is not set in Preferences");
            }
            
            if (string.IsNullOrEmpty(m_owner))
            {
                errors.Add("Owner is not set");
            }
            
            if (string.IsNullOrEmpty(m_repo))
            {
                errors.Add("Repo is not set");
            }
            
            if (string.IsNullOrEmpty(m_releaseName))
            {
                errors.Add("Release Name is not set");
            }
            
            if (string.IsNullOrEmpty(m_tagName))
            {
                errors.Add("Tag Name is not set");
            }
            
            if (string.IsNullOrEmpty(m_target))
            {
                errors.Add("Target is not set");
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