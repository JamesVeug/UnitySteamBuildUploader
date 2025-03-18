namespace Wireframe
{
    internal abstract class AService
    {
        public virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out string reason);
        public abstract void PreferencesGUI();
        public abstract void ProjectSettingsGUI();
    }
}
