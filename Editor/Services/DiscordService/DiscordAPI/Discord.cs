using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// TODO: Move requests to a wrapper
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Wireframe
{
    /// <summary>
    /// https://discord.com/developers/applications
    /// </summary>
    internal partial class Discord
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("discord_enabled");
            set => ProjectEditorPrefs.SetBool("discord_enabled", value);
        }

        /// <summary>
        /// https://discord.com/developers/docs/resources/message#create-message-jsonform-params
        /// </summary>
        public static async Task<bool> SendMessageToChannel(long channelID, string text, string token, bool isBot, List<Dictionary<string, object>> embeds = null, UploadTaskReport.StepResult result = null)
        {
            string url = $"https://discord.com/api/v10/channels/{channelID}/messages";
            Dictionary<string, object> messageData = new Dictionary<string, object>
            {
                { "content", text },
            };

            if (embeds != null && embeds.Count > 0)
            {
                messageData["embeds"] = embeds;
            }

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JSON.SerializeObject(messageData));
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");
                if (isBot)
                {
                    www.SetRequestHeader("Authorization", $"Bot {token}");
                }
                else
                {
                    www.SetRequestHeader("Authorization", token);
                }

                www.SendWebRequest();
                while (!www.isDone)
                {
                    await Task.Yield();
                }
                
                if (www.isHttpError || www.isNetworkError)
                {
                    string downloadHandlerText = www.downloadHandler.text;
                    result?.AddError($"Failed to send message to channel: {www.responseCode} - {downloadHandlerText}");
                    result?.SetFailed(result.Logs[result.Logs.Count - 1].Message);
                    return false;
                }

                result?.AddLog("Message sent.");
                return true;
            }
        }

        public static void GetMe(string token, bool isBot)
        {
            string url = "https://discord.com/api/v10/users/@me";
            UnityWebRequest request = UnityWebRequest.Get(url);
            if (isBot)
            {
                request.SetRequestHeader("Authorization", $"Bot {token}");
            }
            else
            {
                request.SetRequestHeader("Authorization", token);
            }
            
            request.SendWebRequest().completed += _ =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Response: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Error: " + request.error);
                }
            };
        }

        public static void GetServers(string token, bool isBot)
        {
            string url = $"https://discord.com/api/v10/@me/guids";
            UnityWebRequest request = UnityWebRequest.Get(url);
            if (isBot)
            {
                request.SetRequestHeader("Authorization", $"Bot {token}");
            }
            else
            {
                request.SetRequestHeader("Authorization", token);
            }
            
            request.SendWebRequest().completed += _ =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Response: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Error: " + request.error);
                }
            };
        }
    }
}