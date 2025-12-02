using System;
using System.Collections.Generic;

namespace Wireframe
{
    internal partial class WindowUploadTab : StringFormatter.IContextModifier
    {
        private static Dictionary<string, Func<WindowUploadTab, string>> s_StringGetters = new Dictionary<string, Func<WindowUploadTab, string>>()
        {
            
        };
        
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            if (s_StringGetters.TryGetValue(key, out var func))
            {
                value = func(this);
                return true;
            }
            
            if (m_currentUploadProfile != null)
            {
                foreach (UploadConfig config in m_currentUploadProfile.UploadConfigs)
                {
                    if (config.ReplaceString(key, out value, ctx))
                    {
                        return true;
                    }
                }
            }

            value = string.Empty;
            return false;
        }
    }
}