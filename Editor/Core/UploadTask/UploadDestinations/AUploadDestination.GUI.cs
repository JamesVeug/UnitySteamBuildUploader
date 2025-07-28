using System.Reflection;

namespace Wireframe
{
    public abstract partial class AUploadDestination
    {
        public string DisplayName => GetType().GetCustomAttribute<UploadDestinationAttribute>()?.DisplayName ?? GetType().Name;

        protected internal abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
        protected internal abstract void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx);
    }
}