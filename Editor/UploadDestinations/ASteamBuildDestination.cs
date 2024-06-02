using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public abstract class ASteamBuildDestination
    {
        protected float m_uploadProgress;
        protected string m_progressDescription;
        protected bool m_uploadInProgress;

        protected SteamBuildWindow m_window;

        public ASteamBuildDestination(SteamBuildWindow window)
        {
            m_window = window;
        }

        public bool IsRunning => m_uploadInProgress;

        public abstract IEnumerator Upload(string filePath, string buildDescription);
        public abstract string ProgressTitle();
        public abstract bool IsSetup();
        public abstract bool WasUploadSuccessful();
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> s);

        public abstract void OnGUIExpanded(ref bool isDirty);
        public abstract void OnGUICollapsed(ref bool isDirty);

        public virtual void CleanUp()
        {
            m_uploadProgress = 0.0f;
            m_uploadInProgress = false;
        }

        public virtual float UploadProgress()
        {
            return m_uploadProgress;
        }

        public virtual string ProgressDescription()
        {
            return m_progressDescription;
        }

    }
}