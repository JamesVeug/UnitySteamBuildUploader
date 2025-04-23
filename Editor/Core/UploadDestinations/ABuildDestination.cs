using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract class ABuildDestination : DropdownElement
   {
        protected float m_uploadProgress;
        protected string m_progressDescription;
        protected bool m_uploadInProgress;
        internal BuildUploaderWindow uploaderWindow;
        
        internal ABuildDestination(BuildUploaderWindow window)
        {
            // Required for Reflection
            uploaderWindow = window;
        }

        public bool IsRunning => m_uploadInProgress;
        public int Id { get; set;  }

        public virtual Task<bool> Prepare(BuildTaskReport.StepResult result)
        {
            return Task.FromResult(true);
        }
        
        public abstract string DisplayName { get; }
        public abstract Task<bool> Upload(string filePath, string buildDescription, BuildTaskReport.StepResult result);
        public abstract string ProgressTitle();
        public abstract bool IsSetup(out string reason);
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> s);

        public abstract void OnGUIExpanded(ref bool isDirty);
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth);

        public virtual void CleanUp(BuildTaskReport.StepResult result)
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

        public virtual void PostUpload(BuildTaskReport.StepResult report)
        {
            
        }
   }
}