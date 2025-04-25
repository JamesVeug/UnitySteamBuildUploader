using System.Reflection;

namespace Wireframe
{
    public abstract partial class ABuildDestination
    {
        string DropdownElement.DisplayName =>
            GetType().GetCustomAttribute<BuildDestinationAttribute>()?.DisplayName ?? GetType().Name;

        protected internal abstract void OnGUIExpanded(ref bool isDirty);
        protected internal abstract void OnGUICollapsed(ref bool isDirty, float maxWidth);
    }
}