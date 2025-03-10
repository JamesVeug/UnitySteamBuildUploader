using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    internal static class Github
    {
        public static bool Enabled
        {
            get => EditorPrefs.GetBool("github_enabled", false);
            set => EditorPrefs.SetBool("github_enabled", value);
        }

        private static string TokenKey => Application.productName + "GithubTBuildUploader";
        public static string Token
        {
            get => EncodedEditorPrefs.GetString(TokenKey, "");
            set => EncodedEditorPrefs.SetString(TokenKey, value);
        }

        /// <summary>
        /// https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#create-a-release
        /// </summary>
        public static async Task<UploadResult> NewRelease(string owner,
            string repo,
            string releaseName,
            string releaseBody,
            string tagName,
            string target,
            bool draft,
            bool prerelease,
            string token,
            List<string> assets = null)
        {
            // Verify paths first
            foreach (string asset in assets)
            {
                if (!File.Exists(asset) && !Directory.Exists(asset))
                {
                    Debug.LogError($"Path not found: {asset}");
                    return UploadResult.Failed("Path to asset not found: " + asset);
                }
            }

            string url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            Dictionary<string, object> payload = new Dictionary<string,object>()
            {
                {"tag_name", tagName},
                {"target_commitish", target},
                {"name", releaseName},
                {"body", releaseBody},
                {"draft", draft},
                {"prerelease", prerelease},
            };
            
            string jsonPayload = JSON.SerializeObject(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            
            using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                
                www.SetRequestHeader("Accept", "application/vnd.github+json");
                www.SetRequestHeader("Authorization", $"Bearer {token}");
                www.SetRequestHeader("X-GitHub-Api-Version", "2022-11-28");
                www.SetRequestHeader("User-Agent", "Unity-GitHub-Client");
                www.SetRequestHeader("Content-Type", "application/json");
                
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (www.result == UnityWebRequest.Result.Success && www.responseCode == 201)
                {
                    Debug.Log("Release created successfully.");
                    string jsonResponse = www.downloadHandler.text;
                    var release = JSON.DeserializeObject<Dictionary<string, object>>(jsonResponse);
                    string uploadUrl = release["upload_url"].ToString().Split('{')[0];
                    foreach (string assetPath in assets)
                    {
                        UploadResult result = await UploadReleaseAsset(uploadUrl, token, assetPath);
                        if (!result.Successful)
                        {
                            return result;
                        }
                    }
                    Debug.Log("All assets uploaded successfully.");
                }
                else
                {
                    string downloadHandlerText = www.downloadHandler.text;
                    Debug.LogError($"Failed to create release: {www.responseCode} - {downloadHandlerText}");
                    return UploadResult.Failed("Failed to create release: " + www.responseCode + " - " + downloadHandlerText);
                }
            }
            
            return UploadResult.Success();
        }
        
        private static async Task<UploadResult> UploadReleaseAsset(string uploadUrl, string token, string path)
        {
            try
            {
                string assetName = Path.GetFileName(path);
                byte[] fileContent = null;
                if (File.Exists(path))
                {
                    fileContent = await File.ReadAllBytesAsync(path);
                }
                else if(Directory.Exists(path))
                {
                    // Zip the directory
                    string zipPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".zip");
                    if (!await ZipUtils.Zip(path, zipPath))
                    {
                        return UploadResult.Failed("Failed to zip directory: " + path);
                    }

                    fileContent = await File.ReadAllBytesAsync(zipPath);
                    assetName += ".zip";
                }
                else
                {
                    Debug.LogError($"Invalid Path: {path}");
                    return UploadResult.Failed("Invalid Path: " + path);
                }

                string url = $"{uploadUrl}?name={assetName}";

                using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
                {
                    www.uploadHandler = new UploadHandlerRaw(fileContent);
                    www.downloadHandler = new DownloadHandlerBuffer();

                    www.SetRequestHeader("Accept", "application/vnd.github+json");
                    www.SetRequestHeader("Authorization", $"Bearer {token}");
                    www.SetRequestHeader("Content-Type", "application/octet-stream");

                    var operation = www.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    Debug.Log("Upload asset result: " + www.responseCode + " - " + www.downloadHandler.text);
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("File uploaded successfully.");
                        return UploadResult.Success();
                    }
                    else
                    {
                        Debug.LogError($"Failed to upload file: {www.responseCode} - {www.downloadHandler.text}");
                        return UploadResult.Failed("Failed to upload file: " + path + " - " + www.responseCode + " - " + www.downloadHandler.text);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fully create release: {e.Message}");
                return UploadResult.Failed("Failed to fully create release: " + e.Message);
            }
        }
    }
}