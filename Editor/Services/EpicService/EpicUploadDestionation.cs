using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    [Wiki("EpicNewRelease", "destinations", "Upload an artifact to Epic Games.")]
    [UploadDestination("EpicNewRelease")]
    public partial class EpicUploadDestionation : AUploadDestination
    {
        [Wiki("EpicFields", "All CLI arguments required for uploading to Epic Games.")]
        private List<UploadDestinationStringWrapper> Fields = new()
        {
            new("Organization ID", "organizationId", "-OrganizationId="),
            new("Product ID", "productId", "-ProductId="),
            new("Artifact ID", "artifactId", "-ArtifactId="),
            new("Client ID", "clientId", "-ClientId="),
            new("Client Secret", "clientSecret", "-ClientSecret="),
            new("Cloud Dir", "cloudDir", "-CloudDir=",true,true),
            new("Build Version", "buildVersion", "-BuildVersion="),
            new("App Launch", "appLaunch", "-AppLaunch="),
            new("App Args", "appArgs", "-AppArgs=",false)
        };

        public EpicUploadDestionation() : base() { }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            Dictionary<string, string> formatted = new();

            foreach (var f in Fields) formatted[f.InternalName] = StringFormatter.FormatString(f.Value, ctx);

            formatted["buildRoot"] = StringFormatter.FormatString(m_cachedFolderPath, ctx);

            string args = string.Empty;

            foreach (var f in Fields) if(!f.skip) args += $"{f.CliArg}\"{formatted[f.InternalName]}\" ";

            args += $"-mode=UploadBinary -BuildRoot=\"{formatted["buildRoot"]}\" -CloudDir=\"{formatted["cloudDir"].TrimEnd('\\', '/')}\" ";

            UnityEngine.Debug.Log($"[EpicUpload] Final BuildPatchTool command:\n{Epic.SDKPath} {args}");

            return await Process_Utilities.RunTask(result,Epic.SDKPath, args);
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            foreach (var f in Fields)
            {
                string val = StringFormatter.FormatString(f.Value, ctx);

                if (string.IsNullOrEmpty(val) && f.required) errors.Add($"{f.DisplayName} is not set.");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new();
            
            foreach (var f in Fields) dict[f.InternalName] = f.Value;
            
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            foreach (var f in Fields)
            {
                if (s.TryGetValue(f.InternalName, out var val)) f.Value = val as string;
            }
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            EditorGUILayout.LabelField("Epic Games Destination", EditorStyles.boldLabel);
        }

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            if (GUILayout.Button("Docs", GUILayout.Width(50))) Application.OpenURL("https://dev.epicgames.com/docs");

            foreach (var f in Fields)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(f.DisplayName + ":", GUILayout.Width(160));
                    
                    string val = f.Value;
                    
                    bool show = f.ShowFormatted;
                    
                    if (EditorUtils.FormatStringTextField(ref val, ref show, ctx))
                    {
                        f.Value = val;

                        f.ShowFormatted = show;

                        isDirty = true;
                    }
                }
            }
        }
    }
}