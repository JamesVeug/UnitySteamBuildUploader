using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class BuildConfigSource : StringFormatter.IContextModifier
    {
        private static Dictionary<string, Func<BuildConfigSource, string>> s_StringGetters = new()
        {
            { StringFormatter.PRODUCT_NAME_KEY, (b) => b.BuildConfig.ProductName },
            { StringFormatter.BUILD_TARGET_KEY, (b) => b.ResultingPlatform()?.DisplayName ?? "<Unknown Platform>" },
            { StringFormatter.BUILD_TARGET_GROUP_KEY, (b) => b.ResultingTargetGroup().ToString() },
            { StringFormatter.SCRIPTING_BACKEND_KEY, (b) => b.BuildConfig.ScriptingBackend.ToString() },
            { StringFormatter.BUILD_NAME_KEY, (b) => b.BuildConfig.BuildName }
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
            
            if (BuildConfig != null && s_StringGetters.TryGetValue(key, out var func))
            {
                value = func(this);
                return true;
            }

            value = null;
            return false;
        }
    }
}