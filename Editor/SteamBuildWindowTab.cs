using UnityEngine;

namespace Wireframe
{
    internal abstract class SteamBuildWindowTab
    {
        protected SteamBuildWindow window;
        public abstract string TabName { get; }

        public virtual bool Enabled => true;

        public void Initialize(SteamBuildWindow window)
        {
            this.window = window;
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