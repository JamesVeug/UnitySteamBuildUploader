using System.Reflection;

namespace Wireframe
{
    public abstract partial class AUploadSource
    {
        public string DisplayName => GetType().GetCustomAttribute<UploadSourceAttribute>()?.DisplayName ?? GetType().Name;
        public abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx);
    }
}