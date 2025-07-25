using System.Collections.Generic;
using System.Reflection;

namespace Wireframe
{
    public abstract partial class ABuildAction : DropdownElement
    {
        public string DisplayName => GetType().GetCustomAttribute<BuildActionAttribute>()?.DisplayName ?? GetType().Name;
        
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx);
        public abstract void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx);
    }
}