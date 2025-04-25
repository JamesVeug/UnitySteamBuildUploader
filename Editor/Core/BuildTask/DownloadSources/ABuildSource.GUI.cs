using System.Reflection;

namespace Wireframe
{
    public abstract partial class ABuildSource
    {
        public string DisplayName => GetType().GetCustomAttribute<BuildSourceAttribute>()?.DisplayName ?? GetType().Name;
        public abstract void OnGUIExpanded(ref bool isDirty);
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth);
    }
}