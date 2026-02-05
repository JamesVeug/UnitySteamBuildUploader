using System.Collections.Generic;
using UnityEditor;

namespace Wireframe
{
    public interface IBuildConfig
    {
        string GetBuildName { get; }
        string GetGUID { get; }
        string GetProductName { get; }
        List<string> GetSceneGUIDs { get; }
        BuildTargetGroup GetTargetPlatform { get; }
        int GetTargetPlatformSubTarget { get; }
        BuildTarget GetTarget { get; }
        BuildUtils.Architecture GetTargetArchitecture { get; }
        bool GetSwitchTargetPlatform { get; }
        ScriptingImplementation GetScriptingBackend { get; }
        string GetProductExtension();
        BuildOptions GetBuildOptions();
        string GetFormattedProductName(Context ctx);
        bool ApplySettings(bool switchPlatform, Context context, UploadTaskReport.StepResult stepResult = null);
    }
}
