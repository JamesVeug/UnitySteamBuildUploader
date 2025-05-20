using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class ABuildSource : DropdownElement
    {
        public bool IsRunning => m_getSourceInProgress;
        
        protected float m_downloadProgress;
        protected string m_progressDescription;
        protected bool m_getSourceInProgress;


        public ABuildSource() : base()
        {
            // Required for reflection
        }

        public int Id { get; set; }
        public abstract Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult);
        public abstract string SourceFilePath();
        public abstract float DownloadProgress();
        public abstract string ProgressTitle();
        public abstract string ProgressDescription();
        public abstract string GetBuildDescription();

        public virtual void CleanUp(BuildTaskReport.StepResult result)
        {
            m_downloadProgress = 0.0f;
            m_getSourceInProgress = false;
        }

        public virtual void TryGetWarnings(List<string> warnings)
        {
            
        }

        public virtual void TryGetErrors(List<string> errors)
        {
            
        }


        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}