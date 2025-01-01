using UnityEngine;

namespace Wireframe
{
    public abstract class SteamBuildWindowTab
    {
        protected SteamBuildWindow window;
        public abstract string TabName { get; }

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