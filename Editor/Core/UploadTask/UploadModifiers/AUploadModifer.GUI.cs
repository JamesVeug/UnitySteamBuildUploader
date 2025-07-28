using System.Reflection;

namespace Wireframe
{
    public abstract partial class AUploadModifer
    {
        public string DisplayName => GetType().GetCustomAttribute<UploadModifierAttribute>()?.DisplayName ?? GetType().Name;
        protected internal abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
    }
}