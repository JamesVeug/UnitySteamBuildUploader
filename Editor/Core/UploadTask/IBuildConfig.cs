using System.Collections.Generic;
using UnityEditor;

namespace Wireframe
{
    public interface IBuildConfig
    {
        public string GetBuildName { get; }
        public string GetGUID { get; }
        public string GetProductName { get; }
        public List<string> GetSceneGUIDs { get; }
        public BuildTargetGroup GetTargetPlatform { get; }
        public int GetTargetPlatformSubTarget { get; }
        public BuildTarget GetTarget { get; }
        public BuildUtils.Architecture GetTargetArchitecture { get; }
        public bool GetSwitchTargetPlatform { get; }
        public ScriptingImplementation GetScriptingBackend { get; }
        public BuildOptions GetBuildOptions();
        public string GetFormattedProductName(Context ctx);
        public bool ApplySettings(bool switchPlatform, Context context, UploadTaskReport.StepResult stepResult = null);
    }
}