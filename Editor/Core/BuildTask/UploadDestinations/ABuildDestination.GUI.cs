using System.Reflection;

namespace Wireframe
{
    public abstract partial class ABuildDestination
    {
        public string DisplayName => GetType().GetCustomAttribute<BuildDestinationAttribute>()?.DisplayName ?? GetType().Name;

        protected internal abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
        protected internal abstract void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx);
    }
}