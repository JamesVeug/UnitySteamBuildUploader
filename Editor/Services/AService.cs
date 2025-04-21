namespace Wireframe
{
    public abstract class AService
    {
        internal virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out string reason);
        public abstract void PreferencesGUI();
        public abstract void ProjectSettingsGUI();
    }
}
