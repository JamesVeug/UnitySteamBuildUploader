using System.Collections;
using System.Collections.Generic;

namespace Wireframe
{
    public abstract class ASteamBuildSource
    {
        protected float m_downloadProgress;
        protected string m_progressDescription;
        protected bool m_getSourceInProgress;

        public bool IsRunning => m_getSourceInProgress;

        public abstract void OnGUIExpanded();
        public abstract void OnGUICollapsed();
        public abstract IEnumerator GetSource();
        public abstract string SourceFilePath();
        public abstract float DownloadProgress();
        public abstract string ProgressTitle();
        public abstract string ProgressDescription();
        public abstract bool IsSetup();
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