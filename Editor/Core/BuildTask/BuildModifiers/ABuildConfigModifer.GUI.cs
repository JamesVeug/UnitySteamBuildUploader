using System.Reflection;

namespace Wireframe
{
    public abstract partial class ABuildConfigModifer
    {
        public string DisplayName => GetType().GetCustomAttribute<BuildModifierAttribute>()?.DisplayName ?? GetType().Name;
        protected internal abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
    }
}