using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    [Wiki("EpicNewRelease", "destinations", "Create a new release on the Epic platform.")]
    [UploadDestination("EpicNewRelease")]
    public partial class EpicUploadDestionation : AUploadDestination
    {
        [Wiki("OrganizationId", "The ID of the Epic organization.")]
        private string OrganizationId;
        private bool m_showFormattedOrganizationId;

        [Wiki("ProductId", "The ID of the product in the Epic ecosystem.")]
        private string ProductId;
        private bool m_showFormattedProductId;

        [Wiki("ArtifactId", "The artifact identifier for the uploaded build.")]
        private string ArtifactId;
        private bool m_showFormattedArtifactId;

        [Wiki("ClientId", "The Epic Client ID used for authentication.")]
        private string ClientId;
        private bool m_showFormattedClientId;

        [Wiki("ClientSecretEnvVar", "The environment variable name containing the Epic Client Secret.")]
        private string ClientSecretEnvVar;
        private bool m_showFormattedClientSecretEnvVar;

        [Wiki("CloudDir", "Directory on Epic’s cloud storage where files will be uploaded.")]
        private string CloudDir;
        private bool m_showFormattedCloudDir;

        [Wiki("BuildVersion", "Version string for the build.")]
        private string BuildVersion;
        private bool m_showFormattedBuildVersion;

        [Wiki("AppLaunch", "Executable to launch for the uploaded app.")]
        private string AppLaunch;
        private bool m_showFormattedAppLaunch;

        [Wiki("AppArgs", "Arguments to pass to the app when launching.")]
        private string AppArgs;
        private bool m_showFormattedAppArgs;

        public EpicUploadDestionation() : base() { }

        public EpicUploadDestionation(
            string organizationId,
            string productId,
            string artifactId,
            string clientId,
            string clientSecretEnvVar,
            string buildRoot,
            string cloudDir,
            string buildVersion,
            string appLaunch,
            string appArgs,
            bool showFormattedOrganizationId,
            bool showFormattedProductId,
            bool showFormattedArtifactId,
            bool showFormattedClientId,
            bool showFormattedClientSecretEnvVar,
            bool showFormattedBuildRoot,
            bool showFormattedCloudDir,
            bool showFormattedBuildVersion,
            bool showFormattedAppLaunch,
            bool showFormattedAppArgs)
        {
            OrganizationId = organizationId;
            ProductId = productId;
            ArtifactId = artifactId;
            ClientId = clientId;
            ClientSecretEnvVar = clientSecretEnvVar;
            CloudDir = cloudDir;
            BuildVersion = buildVersion;
            AppLaunch = appLaunch;
            AppArgs = appArgs;

            m_showFormattedOrganizationId = showFormattedOrganizationId;
            m_showFormattedProductId = showFormattedProductId;
            m_showFormattedArtifactId = showFormattedArtifactId;
            m_showFormattedClientId = showFormattedClientId;
            m_showFormattedClientSecretEnvVar = showFormattedClientSecretEnvVar;
            m_showFormattedCloudDir = showFormattedCloudDir;
            m_showFormattedBuildVersion = showFormattedBuildVersion;
            m_showFormattedAppLaunch = showFormattedAppLaunch;
            m_showFormattedAppArgs = showFormattedAppArgs;
        }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            // Format everything once and store it
            string orgId = StringFormatter.FormatString(OrganizationId, ctx);
            string productId = StringFormatter.FormatString(ProductId, ctx);
            string artifactId = StringFormatter.FormatString(ArtifactId, ctx);
            string clientId = StringFormatter.FormatString(ClientId, ctx);
            string clientSecretEnv = StringFormatter.FormatString(ClientSecretEnvVar, ctx);
            string buildRoot = StringFormatter.FormatString(m_cachedFolderPath, ctx);
            string cloudDir = StringFormatter.FormatString(CloudDir, ctx);
            string buildVer = StringFormatter.FormatString(BuildVersion, ctx);
            string appLaunch = StringFormatter.FormatString(AppLaunch, ctx);
            string appArgs = StringFormatter.FormatString(AppArgs, ctx);

            // Log each individually to catch blanks or nulls
            UnityEngine.Debug.Log(
                $"[EpicUpload] Formatted arguments:\n" +
                $"OrganizationId: {orgId}\n" +
                $"ProductId: {productId}\n" +
                $"ArtifactId: {artifactId}\n" +
                $"ClientId: {clientId}\n" +
                $"ClientSecret: {clientSecretEnv}\n" +
                $"BuildRoot: {buildRoot}\n" +
                $"CloudDir: {cloudDir}\n" +
                $"BuildVersion: {buildVer}\n" +
                $"AppLaunch: {appLaunch}\n" +
                $"AppArgs: {appArgs}"
            );

            string args =
                $"-OrganizationId=\"{orgId}\" " +
                $"-ProductId=\"{productId}\" " +
                $"-ArtifactId=\"{artifactId}\" " +
                $"-ClientId=\"{clientId}\" " +
                $"-ClientSecret=\"{clientSecretEnv}\" " +
                $"-mode=UploadBinary " +                  // lowercase 'mode' per your doc
                $"-BuildRoot=\"{buildRoot}\" " +
                $"-CloudDir=\"{cloudDir.TrimEnd('\\', '/')}\" " +  // trim trailing slash
                $"-BuildVersion=\"{buildVer}\" " +
                $"-AppLaunch=\"{appLaunch}\" " +
                $"-AppArgs=\"{appArgs}\"";


            UnityEngine.Debug.Log($"[EpicUpload] Final BuildPatchTool command:\n{Epic.SDKPath} {args}");

            var startInfo = new ProcessStartInfo
            {
                FileName = Epic.SDKPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.SetFailed("Failed to start BuildPatchTool process (process is null).");
                    return false;
                }

                string output = await process.StandardOutput.ReadToEndAsync();
                string errors = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                UnityEngine.Debug.Log($"[EpicUpload] Output:\n{output}");
                if (!string.IsNullOrEmpty(errors))
                    UnityEngine.Debug.LogError($"[EpicUpload] Errors:\n{errors}");

                if (process.ExitCode != 0)
                {
                    result.SetFailed($"BuildPatchTool exited with code {process.ExitCode}.\nSee log for details.");
                    return false;
                }

                return result.Successful;
            }
            catch (Exception ex)
            {
                result.AddException(ex);
                result.SetFailed($"Failed to run BuildPatchTool: {ex.Message}");
                return false;
            }
        }


        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            if (string.IsNullOrEmpty(OrganizationId))
                errors.Add("Organization ID is not set.");
            if (string.IsNullOrEmpty(ProductId))
                errors.Add("Product ID is not set.");
            if (string.IsNullOrEmpty(ArtifactId))
                errors.Add("Artifact ID is not set.");
            if (string.IsNullOrEmpty(ClientId))
                errors.Add("Client ID is not set.");
            if (string.IsNullOrEmpty(ClientSecretEnvVar))
                errors.Add("Client Secret Env Var is not set.");
            if (string.IsNullOrEmpty(CloudDir))
                errors.Add("Cloud Dir is not set.");
        }

        public override Dictionary<string, object> Serialize()
        {
            return new()
            {
                { "organizationId", OrganizationId },
                { "productId", ProductId },
                { "artifactId", ArtifactId },
                { "clientId", ClientId },
                { "clientSecretEnvVar", ClientSecretEnvVar },
                { "cloudDir", CloudDir },
                { "buildVersion", BuildVersion },
                { "appLaunch", AppLaunch },
                { "appArgs", AppArgs }
            };
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            OrganizationId = s.TryGetValue("organizationId", out var org) ? org as string : null;
            ProductId = s.TryGetValue("productId", out var prod) ? prod as string : null;
            ArtifactId = s.TryGetValue("artifactId", out var art) ? art as string : null;
            ClientId = s.TryGetValue("clientId", out var cid) ? cid as string : null;
            ClientSecretEnvVar = s.TryGetValue("clientSecretEnvVar", out var env) ? env as string : null;
            CloudDir = s.TryGetValue("cloudDir", out var cloud) ? cloud as string : null;
            BuildVersion = s.TryGetValue("buildVersion", out var ver) ? ver as string : null;
            AppLaunch = s.TryGetValue("appLaunch", out var app) ? app as string : null;
            AppArgs = s.TryGetValue("appArgs", out var args) ? args as string : null;
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            string text = $"{OrganizationId}/{ProductId}/{ArtifactId}/{BuildVersion}";
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            if (GUILayout.Button("Docs", GUILayout.Width(50)))
                Application.OpenURL("https://dev.epicgames.com/docs");

            DrawFormattedField("Organization ID", ref OrganizationId, ref m_showFormattedOrganizationId, ref isDirty, ctx);
            DrawFormattedField("Product ID", ref ProductId, ref m_showFormattedProductId, ref isDirty, ctx);
            DrawFormattedField("Artifact ID", ref ArtifactId, ref m_showFormattedArtifactId, ref isDirty, ctx);
            DrawFormattedField("Client ID", ref ClientId, ref m_showFormattedClientId, ref isDirty, ctx);
            DrawFormattedField("Client Secret Env Var", ref ClientSecretEnvVar, ref m_showFormattedClientSecretEnvVar, ref isDirty, ctx);
            DrawFormattedField("Cloud Dir", ref CloudDir, ref m_showFormattedCloudDir, ref isDirty, ctx);
            DrawFormattedField("Build Version", ref BuildVersion, ref m_showFormattedBuildVersion, ref isDirty, ctx);
            DrawFormattedField("App Launch", ref AppLaunch, ref m_showFormattedAppLaunch, ref isDirty, ctx);
            DrawFormattedField("App Args", ref AppArgs, ref m_showFormattedAppArgs, ref isDirty, ctx);
        }

        private void DrawFormattedField(string label, ref string value, ref bool showFormatted, ref bool isDirty, StringFormatter.Context ctx)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label + ":", GUILayout.Width(160));
                isDirty |= EditorUtils.FormatStringTextField(ref value, ref showFormatted, ctx);
            }
        }
    }
}
