using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal abstract class ABuildSource : DropdownElement
    {
        protected float m_downloadProgress;
        protected string m_progressDescription;
        protected bool m_getSourceInProgress;
        protected BuildUploaderWindow uploaderWindow;
        
        public ABuildSource(BuildUploaderWindow window)
        {
            // Required for Reflection
            uploaderWindow = window;
        }

        public bool IsRunning => m_getSourceInProgress;

        public int Id { get; set; }
        public abstract string DisplayName { get; }
        public abstract void OnGUIExpanded(ref bool isDirty);
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth);
        public abstract Task<bool> GetSource();
        public abstract string SourceFilePath();
        public abstract float DownloadProgress();
        public abstract string ProgressTitle();
        public abstract string ProgressDescription();
        public abstract bool IsSetup(out string reason);
        public abstract string GetBuildDescription();

        public virtual void CleanUp()
        {
            m_downloadProgress = 0.0f;
            m_getSourceInProgress = false;
        }


        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);

        public virtual void AssignLatestBuildTarget()
        {

        }
    }
}