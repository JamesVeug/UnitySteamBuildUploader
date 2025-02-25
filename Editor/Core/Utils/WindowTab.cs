namespace Wireframe
{
    internal abstract class WindowTab
    {
        protected BuildUploaderWindow UploaderWindow;
        public abstract string TabName { get; }

        public virtual bool Enabled => true;

        public void Initialize(BuildUploaderWindow uploaderWindow)
        {
            this.UploaderWindow = uploaderWindow;
        }

        public virtual void OnGUI()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Save()
        {

        }
    }
}