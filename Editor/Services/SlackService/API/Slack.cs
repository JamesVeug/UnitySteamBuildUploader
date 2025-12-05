using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Wireframe
{
    public static class Slack
    {
        public class Attachment
        {
            public class Action
            {
                public string name;
                public string text;
                public string type;
                public string data_source;
            }
            
            public string text;
            public string fallback;
            public string color;
            public string attachment_type;
            public string callback_id;
            public List<Action> actions = new List<Action>();

            public Dictionary<string, object> Serialize()
            {
                Dictionary<string, object> attachmentDict = new Dictionary<string, object>
                {
                    { "text", text },
                    { "fallback", fallback },
                    { "color", color },
                    { "attachment_type", attachment_type },
                    { "callback_id", callback_id },
                };
                    
                List<Dictionary<string, string>> attachmentsList2 = new List<Dictionary<string, string>>();
                foreach (Action action in actions)
                {
                    Dictionary<string, string> actionDict = new Dictionary<string, string>
                    {
                        { "name", action.name },
                        { "text", action.text },
                        { "type", action.type },
                        { "data_source", action.data_source },
                    };
                    attachmentsList2.Add(actionDict);
                }
                attachmentDict["actions"] = attachmentsList2;
                return attachmentDict;
            }
            
            public static Attachment Deserialize(Dictionary<string, object> dict)
            {
                Attachment attachment = new Attachment();
                attachment.text = dict.ContainsKey("text") ? dict["text"].ToString() : "";
                attachment.fallback = dict.ContainsKey("fallback") ? dict["fallback"].ToString() : "";
                attachment.color = dict.ContainsKey("color") ? dict["color"].ToString() : "";
                attachment.attachment_type = dict.ContainsKey("attachment_type") ? dict["attachment_type"].ToString() : "";
                attachment.callback_id = dict.ContainsKey("callback_id") ? dict["callback_id"].ToString() : "";
                
                if (dict.ContainsKey("actions"))
                {
                    List<object> actionsList = dict["actions"] as List<object>;
                    if (actionsList != null)
                    {
                        foreach (object actionObj in actionsList)
                        {
                            Dictionary<string, object> actionDict = actionObj as Dictionary<string, object>;
                            if (actionDict != null)
                            {
                                Action action = new Action();
                                action.name = actionDict.ContainsKey("name") ? actionDict["name"].ToString() : "";
                                action.text = actionDict.ContainsKey("text") ? actionDict["text"].ToString() : "";
                                action.type = actionDict.ContainsKey("type") ? actionDict["type"].ToString() : "";
                                action.data_source = actionDict.ContainsKey("data_source") ? actionDict["data_source"].ToString() : "";
                                attachment.actions.Add(action);
                            }
                        }
                    }
                }

                return attachment;
            }
            
        }
        
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("slack_enabled", false);
            set => ProjectEditorPrefs.SetBool("slack_enabled", value);
        }

        /// <summary>
        /// https://api.slack.com/apps/
        /// https://docs.slack.dev/apis/web-api/
        /// https://docs.slack.dev/app-management/quickstart-app-settings/
        /// https://docs.slack.dev/reference/methods/chat.postmessage
        /// </summary>
        public static async Task<bool> SendMessage(string text, string channel, string token, List<Attachment> attachments, UploadTaskReport.StepResult result = null)
        {
            /*
            POST /api/chat.postMessage
            Content-type: application/json
            Authorization: Bearer xoxp-xxxxxxxxx-xxxx
            {   
                "channel":"xxxxxxxxxx",
                "text":"I hope the tour went well, Mr. Wonka.",
                "attachments":[
                    {
                        "text":"Who wins the lifetime supply of chocolate?",
                        "fallback":"You could be telling the computer exactly what it can do with a lifetime supply of chocolate.",
                        "color":"#3AA3E3",
                        "attachment_type":"default",
                        "callback_id":"select_simple_1234",
                        "actions":[
                            {
                                "name":"winners_list",
                                "text":"Who should win?",
                                "type":"select",
                                "data_source":"users"
                            }
                        ]
                    }
                ]
            }
            */

            string url = $"https://slack.com/api/chat.postMessage";
            Dictionary<string, object> messageData = new Dictionary<string, object>
            {
                { "channel", channel },
                { "text", text },
            };

            if (attachments != null && attachments.Count > 0)
            {
                List<Dictionary<string, object>> attachmentsList = new List<Dictionary<string, object>>();
                foreach (Attachment attachment in attachments)
                {
                    Dictionary<string, object> attachmentDict = attachment.Serialize();
                    attachmentsList.Add(attachmentDict);
                }
                messageData["attachments"] = attachmentsList;
            }

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JSON.SerializeObject(messageData));
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {token}");

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

                string handlerText = www.downloadHandler.text;
                result?.AddLog(handlerText);
                
                // {
                //     "ok": true,
                //     "channel": "xxxxxxxxxx",
                //     "ts": "1763462862.962839",
                //     "message": {
                //         "user": "xxxxxxxxxx",
                //         "type": "message",
                //         "ts": "1763462862.962839",
                //         "bot_id": "xxxxxxxxxx",
                //         "app_id": "xxxxxxxxxx",
                //         "text": "Hello!",
                //         "team": "xxxxxxxxxx",
                //         "bot_profile": {
                //             "id": "xxxxxxxxxx",
                //             "app_id": "xxxxxxxxxx",
                //             "user_id": "xxxxxxxxxx",
                //             "name": "Build Uploader",
                //             "icons": {
                //                 "image_36": "https://a.slack-edge.com/80588/img/plugins/app/bot_36.png",
                //                 "image_48": "https://a.slack-edge.com/80588/img/plugins/app/bot_48.png",
                //                 "image_72": "https://a.slack-edge.com/80588/img/plugins/app/service_72.png"
                //             },
                //             "deleted": false,
                //             "updated": 1763462201,
                //             "team_id": "xxxxxxxxxx"
                //         },
                //         "blocks": [
                //         {
                //             "type": "rich_text",
                //             "block_id": "A6sd",
                //             "elements": [
                //             {
                //                 "type": "rich_text_section",
                //                 "elements": [
                //                 {
                //                     "type": "text",
                //                     "text": "Hello!"
                //                 }
                //                 ]
                //             }
                //             ]
                //         }
                //         ]
                //     },
                //     "warning": "missing_charset",
                //     "response_metadata": {
                //         "warnings": [
                //         "missing_charset"
                //             ]
                //     }
                // }
                
                // {
                //     "ok": false,
                //     "error": "invalid_auth",
                //     "warning": "missing_charset",
                //     "response_metadata": {
                //         "warnings": [
                //         "missing_charset"
                //             ]
                //     }
                // }
                string json = handlerText;
                Dictionary<string,string> responseDict = JSON.DeserializeObject<Dictionary<string, string>>(json);
                if (responseDict != null)
                {
                    if (responseDict.ContainsKey("ok") && responseDict["ok"] == "false")
                    {
                        string error = responseDict.ContainsKey("error") ? responseDict["error"] : "unknown_error";
                        result?.SetFailed($"Failed to send message to channel: {error}");
                        return false;
                    }
                }

                result?.AddLog("Message Successful");
                return true;
            }
        }
        
    }
}