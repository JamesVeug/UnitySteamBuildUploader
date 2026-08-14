using System.Linq;
using System.Reflection;

namespace Wireframe
{   
    internal static class AssemblyUtils
    {
        public static Assembly[] GetAssemblies()
        {
        #if UNITY_6000_7_OR_NEWER
            return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies().ToArray();
        #else
            return AppDomain.CurrentDomain.GetAssemblies();
        #endif
        }
    }
}
