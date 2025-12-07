namespace Wireframe
{
    public partial class BuildConfigSource
    {
        private BuildConfig BuildConfigContext => m_buildConfigToApply != null ? m_buildConfigToApply : m_BuildConfig;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.PRODUCT_NAME_KEY, () => BuildConfigContext.ProductName);
            context.AddCommand(Context.BUILD_TARGET_KEY, () => ResultingPlatform()?.DisplayName ?? "<Unknown Platform>");
            context.AddCommand(Context.BUILD_ARCHITECTURE_KEY, () => ResultingArchitecture().ToString());
            context.AddCommand(Context.BUILD_TARGET_GROUP_KEY, () => ResultingTargetGroup().ToString());
            context.AddCommand(Context.SCRIPTING_BACKEND_KEY, () => BuildUtils.ScriptingBackendDisplayName(BuildConfigContext.ScriptingBackend));
            context.AddCommand(Context.BUILD_NAME_KEY, () => BuildConfigContext.BuildName);
            context.AddCommand(Context.BUILD_NUMBER_KEY, () =>
            {
                if (m_buildMetaData == null)
                {
                    return (BuildUploaderProjectSettings.Instance.LastBuildNumber + 1).ToString();
                }

                return m_buildMetaData.BuildNumber.ToString();
            });
            return context;
        }
    }
}
