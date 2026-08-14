using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    [Wiki(nameof(GitTagCommitAction), "actions",
        "Tag the commit the project is currently on, optionally pushing the tag to the remote.")]
    [UploadAction("Git Tag Commit")]
    public partial class GitTagCommitAction : AUploadAction
    {
        [Wiki("Tag", "The name of the tag to create. eg: #420 or v1.4.2", 1)]
        private string m_tagFormat = "#" + Context.BUILD_NUMBER_KEY;

        [Wiki("Auto Push", "If true, the tag is pushed to the remote as soon as it is created.", 2)]
        private bool m_push;
            
        [Wiki("Remote", "When pushing tag to git, this is the remote that is used.", 3)]
        private string m_remote = "origin";

        public GitTagCommitAction() : base()
        {
            // Required for reflection
        }

        public override async Task<bool> Execute(UploadTaskReport.StepResult stepResult)
        {
            string tag = m_context.FormatString(m_tagFormat);
            if (string.IsNullOrEmpty(tag))
            {
                stepResult.SetFailed("Tag did not resolve to a value.");
                return false;
            }

            // The tag has to exist locally before it can be pushed - pushing a ref that was never created
            // fails with "src refspec ... does not match any".
            if (!await Git.CreateTag(tag, stepResult))
            {
                return false;
            }

            if (m_push)
            {
                return await Git.PushTag(tag, m_remote, stepResult);
            }

            return true;
        }

        public override void TryGetErrors(List<GUIContent> errors)
        {
            base.TryGetErrors(errors);

            GitService service = InternalUtils.GetService<GitService>();
            if (!service.IsReadyToStartBuild(out GUIContent reason))
            {
                errors.Add(reason);
            }

            if (string.IsNullOrEmpty(m_tagFormat))
            {
                errors.Add(new GUIContent("Tag is not set"));
                return;
            }

            string tag = m_context.FormatString(m_tagFormat);
            if (!Git.IsValidTagName(tag, out string invalidReason))
            {
                errors.Add(new GUIContent($"Bad Git Tag '{tag}': {invalidReason}",
                    "Git tag names cannot contain spaces, control characters, or any of ~ ^ : ? * [ \\, " +
                    "and cannot contain '..' or '@{', end with '.' or '.lock', or start a part with '.'."));
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "tagFormat", m_tagFormat ?? "" },
                { "push", m_push }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_tagFormat = data.TryGetValue("tagFormat", out object tagFormat) ? tagFormat as string ?? "" : "";
            m_push = data.TryGetValue("push", out object push) && (bool)push;
        }
    }
}
