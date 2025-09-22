using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class BuildConfigSource : StringFormatter.IContextModifier
    {
        private BuildConfig BuildConfigContext => m_buildConfigToApply != null ? m_buildConfigToApply : m_BuildConfig;
        private static Dictionary<string, Func<BuildConfigSource, string>> s_StringGetters = new Dictionary<string, Func<BuildConfigSource, string>>()
        {
            { StringFormatter.PRODUCT_NAME_KEY, (b) => b.BuildConfigContext.ProductName },
            { StringFormatter.BUILD_TARGET_KEY, (b) => b.ResultingPlatform()?.DisplayName ?? "<Unknown Platform>" },
            { StringFormatter.BUILD_TARGET_GROUP_KEY, (b) => b.ResultingTargetGroup().ToString() },
            { StringFormatter.SCRIPTING_BACKEND_KEY, (b) => BuildUtils.ScriptingBackendDisplayName(b.BuildConfigContext.ScriptingBackend) },
            { StringFormatter.BUILD_NAME_KEY, (b) => b.BuildConfigContext.BuildName }
        };
        
        public bool ReplaceString(string key, out string value)
        {
            if (key == StringFormatter.BUILD_NUMBER_KEY)
            {
                if (m_buildMetaData == null)
                {
                    value = (BuildUploaderProjectSettings.Instance.LastBuildNumber + 1).ToString();
                }
                else
                {
                    value = m_buildMetaData.BuildNumber.ToString();
                }

                return true;
            }
            
            if (BuildConfigContext != null && s_StringGetters.TryGetValue(key, out var func))
            {
                value = func(this);
                return true;
            }

            value = null;
            return false;
        }
    }
}