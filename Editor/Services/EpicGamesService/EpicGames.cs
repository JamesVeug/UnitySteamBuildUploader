using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static class EpicGames
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("epicgames_enabled", false);
            set => ProjectEditorPrefs.SetBool("epicgames_enabled", value);
        }

        public static string SDKPath
        {
            get => EditorPrefs.GetString("epicgames_sdkpath");
            set => EditorPrefs.SetString("epicgames_sdkpath", value);
        }

        public static string CloudPath
        {
            get => ProjectEditorPrefs.GetString("epicgames_cloudpath", Path.Combine("{cacheFolderPath}", "EpicGamesCloud"));
            set => ProjectEditorPrefs.SetString("epicgames_cloudpath", value);
        }

        public static async Task<bool> Upload(
            UploadTaskReport.StepResult result, 
            StringFormatter.Context ctx,
            string buildPath,
            string organizationID,
            string productID,
            string artifactID,
            string clientID,
            string cloudDir,
            string buildVersion,
            string appLaunch,
            EpicGamesProduct.SecretTypes secretType,
            string clientSecret,
            string appArgs = "")
        {
            string exePath = "";
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                exePath = Path.Combine(SDKPath, "Engine", "Binaries", "Win64", "BuildPatchTool.exe");
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                exePath = Path.Combine(SDKPath, "Engine", "Binaries", "Mac", "BuildPatchTool");
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                exePath = Path.Combine(SDKPath, "Engine", "Binaries", "Linux", "BuildPatchTool");
            }
            else
            {
                result.SetFailed("Unsupported platform for BuildPatchTool: " + Application.platform);
                return false;
            }

            if (!File.Exists(exePath))
            {
                result.SetFailed("BuildPatchTool not found at path: " + exePath);
                return false;
            }
            
            string formattedBuildVersion = StringFormatter.FormatString(buildVersion, ctx).Trim();
            
            string args = "-mode=UploadBinary";
            args += $" -BuildRoot=\"{StringFormatter.FormatString(buildPath, ctx)}\"";
            args += $" -OrganizationId=\"{StringFormatter.FormatString(organizationID, ctx)}\"";
            args += $" -ProductId=\"{StringFormatter.FormatString(productID, ctx)}\"";
            args += $" -ArtifactId=\"{StringFormatter.FormatString(artifactID, ctx)}\"";
            args += $" -ClientId=\"{StringFormatter.FormatString(clientID, ctx)}\"";
            
            switch (secretType)
            {
                case EpicGamesProduct.SecretTypes.EnvVar:
                    args += $" -ClientSecretEnvVar=\"{StringFormatter.FormatString(clientSecret, ctx)}\"";
                    break;
                case EpicGamesProduct.SecretTypes.ClientSecret:
                    args += $" -ClientSecret=\"{StringFormatter.FormatString(clientSecret, ctx)}\"";
                    break;
                default:
                    result.SetFailed("Invalid secret type selected: " + secretType);
                    return false;
            }

            args += $" -CloudDir=\"{StringFormatter.FormatString(cloudDir, ctx)}\"";
            args += $" -BuildVersion=\"{formattedBuildVersion}\"";
            args += $" -AppLaunch=\"{StringFormatter.FormatString(appLaunch, ctx)}\"";
            args += $" -AppArgs=\"{StringFormatter.FormatString(appArgs, ctx)}\"";

            ProcessUtils.ProcessResult uploadResult = await ProcessUtils.RunTask(result, exePath, args);
            if (!uploadResult.Successful)
            {
                if (uploadResult.Output.Contains("BuildVersion string should only contain"))
                {
                    int indexOf = uploadResult.Output.IndexOf("BuildVersion string should only contain", StringComparison.Ordinal);
                    int end = uploadResult.Output.IndexOf('\n', indexOf);
                    string errorMessage = uploadResult.Output.Substring(indexOf, end - indexOf) + ": '" + formattedBuildVersion + "'";
                    result.SetFailed(errorMessage);
                    return false;
                }

                if (uploadResult.Output.Contains("Raw={"))
                {
                    int indexOf = uploadResult.Output.IndexOf("Raw={", StringComparison.Ordinal) + 4;
                    int end = uploadResult.Output.IndexOf('\n', indexOf);
                    
                    // {"errorCode":"errors.com.epicgames.account.invalid_client_credentials",
                    // "errorMessage":"Sorry the client credentials you are using are invalid",
                    // "messageVars":[],
                    // "numericErrorCode":18033,
                    // "originatingService":"com.epicgames.account.public",
                    // "intent":"prod",
                    // "error_description":"Sorry the client credentials you are using are invalid",
                    // "error":"invalid_client"}
                    string json = uploadResult.Output.Substring(indexOf, end - indexOf);
                    Dictionary<string, string> errorData = JSON.DeserializeObject<Dictionary<string, string>>(json);
                    if (errorData != null)
                    {
                        string err = "";
                        if (errorData.TryGetValue("error", out string error) && !string.IsNullOrEmpty(error))
                        {
                            err = error;
                        }
                        if (errorData.TryGetValue("numericErrorCode", out string errorCode) && !string.IsNullOrEmpty(error))
                        {
                            err += $" (Code: {errorCode})";
                        }
                        
                        string errorMessage = "";
                        if ((errorData.TryGetValue("errorMessage", out var message) && !string.IsNullOrEmpty(message))
                            || (errorData.TryGetValue("error_description", out message) && !string.IsNullOrEmpty(message)))
                        {
                            errorMessage = message;
                        }

                        string finalError = "";
                        if (!string.IsNullOrEmpty(err))
                        {
                            finalError = err;
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            if (!string.IsNullOrEmpty(finalError))
                            {
                                finalError += ": ";
                            }
                            finalError += errorMessage;
                        }

                        if (!string.IsNullOrEmpty(finalError))
                        {
                            result.SetFailed(finalError);
                            return false;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(uploadResult.Errors))
                {
                    result.SetFailed(uploadResult.Errors);
                }
                else
                {
                    result.SetFailed("Unhandled reason. Check logs for info");
                }

                return false;
            }

            if (uploadResult.Output.Contains("Successfully registered binary in artifact service"))
            {
                return true;
            }
            
            // TODO: Check for errors that we may have missed
            return true;
        }
    }
}