using System;
using System.Reflection;

namespace Wireframe
{
    public static class TypeUtils
    {
        public static T GetCustomAttribute<T>(this FieldInfo field) where T : System.Attribute
        {
            return (T)Attribute.GetCustomAttribute(field, typeof(T));
        }

        public static bool TryGetCustomAttribute<T>(this FieldInfo field, out T attribute) where T : System.Attribute
        {
            Attribute customAttribute = Attribute.GetCustomAttribute(field, typeof(T));
            if (customAttribute == null)
            {
                attribute = null;
                return false;
            }

            attribute = (T)customAttribute;
            return true;
        }
    }
}