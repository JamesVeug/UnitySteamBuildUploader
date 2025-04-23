using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    /// <summary>
    /// Download something from online
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    public class URLSource : ABuildSource
    {
        public override string DisplayName => "URL";

        private string m_sourcePath;
        
        private string m_url;
        private string m_fileName;
        private WebRequestMethod m_method;
        private List<Tuple<string,string>> m_headers = new List<Tuple<string, string>>();

        public URLSource() : base(null)
        {
            
        }
        
        public void SetURL(string url, WebRequestMethod method)
        {
            m_url = url;
        }
        
        public void SetHeaders(params Tuple<string,string>[] headers)
        {
            m_headers = headers.ToList();
        }
        
        public void AddHeader(string key, string value)
        {
            m_headers.Add(new Tuple<string, string>(key, value));
        }
        
        internal URLSource(BuildUploaderWindow window) : base(window)
        {
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                WebRequestMethod newMethod = (WebRequestMethod)EditorGUILayout.EnumPopup(m_method, GUILayout.Width(80));
                if (m_method != newMethod)
                {
                    m_method = newMethod;
                    isDirty = true;
                }

                float width = maxWidth - 120;
                string truncatedText = Utils.TruncateText(m_url, width, "No URL entered...");
                GUILayout.Label(truncatedText, GUILayout.Width(width));
            }
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("URL:", GUILayout.Width(120));
                string newUrl = GUILayout.TextField(m_url);
                if (m_url != newUrl)
                {
                    m_url = newUrl;
                    isDirty = true;
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Method:", GUILayout.Width(120));
                WebRequestMethod newMethod = (WebRequestMethod)EditorGUILayout.EnumPopup(m_method);
                if (m_method != newMethod)
                {
                    m_method = newMethod;
                    isDirty = true;
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("File Name:", GUILayout.Width(120));
                string newFileName = GUILayout.TextField(m_fileName);
                if (m_fileName != newFileName)
                {
                    m_fileName = newFileName;
                    isDirty = true;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Headers:", GUILayout.Width(120));
                if (m_headers.Count > 0)
                {
                    isDirty |= DrawHeader(m_headers, 0);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("", GUILayout.Width(120));
                using (new EditorGUILayout.VerticalScope())
                {
                    for (var i = 1; i < m_headers.Count; i++)
                    {
                        DrawHeader(m_headers, i);
                    }

                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        m_headers.Add(new Tuple<string, string>("", ""));
                        isDirty = true;
                    }
                }
            }
        }

        private bool DrawHeader(List<Tuple<string,string>> headers, int index)
        {
            var header = headers[index];
            using (new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("X", GUILayout.Width(20)))
                {
                    m_headers.RemoveAt(index);
                    return true;
                }
                    
                string newKey = GUILayout.TextField(header.Item1, GUILayout.Width(120));
                if (header.Item1 != newKey)
                {
                    header = new Tuple<string, string>(newKey, header.Item2);
                    m_headers[index] = header;
                    return true;
                }

                string newValue = GUILayout.TextField(header.Item2);
                if (header.Item2 != newValue)
                {
                    header = new Tuple<string, string>(header.Item1, newValue);
                    m_headers[index] = header;
                    return true;
                }
            }
            
            return false;
        }

        public override async Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult)
        {
            m_getSourceInProgress = true;
            m_downloadProgress = 0.0f;

            // Preparing
            m_progressDescription = "Preparing...";
            string directoryPath = Path.Combine(Utils.CacheFolder, "URLBuilds");
            if (!Directory.Exists(directoryPath))
            {
                stepResult.AddLog("Creating directory: " + directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            string fullFilePath = Path.Combine(directoryPath, m_fileName);

            // Only download if we don't have it
            if (!File.Exists(fullFilePath))
            {
                stepResult.AddLog("Downloading from URL: " + m_url);

                m_progressDescription = "Fetching...";
                UnityWebRequest request = new UnityWebRequest(m_url, m_method.ToString());
                foreach (Tuple<string,string> header in m_headers)
                {
                    request.SetRequestHeader(header.Item1, header.Item2);
                }
                
                UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();

                // Wait for it to be downloaded?
                m_progressDescription = "Downloading from URL...";
                while (!webRequest.isDone)
                {
                    await Task.Delay(10);
                    m_downloadProgress = request.downloadProgress;
                }
                
                if (request.isHttpError || request.isNetworkError)
                {
                    string message = $"Could not download build from url {m_url}:\nError: {request.error}";
                    stepResult.AddError(message);
                    return false;
                }

                // Save
                m_progressDescription = "Saving locally...";
                
#if UNITY_2021_2_OR_NEWER
                await File.WriteAllBytesAsync(fullFilePath, request.downloadHandler.data);
#else
                File.WriteAllBytes(fullFilePath, request.downloadHandler.data);
#endif
            }
            else
            {
                stepResult.AddLog("Skipping downloading from URL since it already exists: " + fullFilePath);
            }

            m_progressDescription = "Done!";

            // Record where the game is saved to
            m_sourcePath = fullFilePath;
            m_downloadProgress = 1.0f;
            stepResult.AddLog("Retrieved URL Build: " + m_sourcePath);
            return true;
        }

        public override string SourceFilePath()
        {
            return m_sourcePath;
        }

        public override float DownloadProgress()
        {
            return m_downloadProgress;
        }

        public override string ProgressTitle()
        {
            return "Downloading from URL";
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup(out string reason)
        {
            if (string.IsNullOrEmpty(m_url))
            {
                reason = "URL not set";
                return false;
            }

            if (string.IsNullOrEmpty(m_fileName))
            {
                reason = "File name not set";
                return false;
            }

            reason = "";
            return true;
        }

        public override string GetBuildDescription()
        {
            return "";
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["url"] = m_url,
                ["fileName"] = m_fileName,
                ["method"] = m_method.ToString()
            };

            Dictionary<string, object> headers = new Dictionary<string, object>();
            foreach (Tuple<string, string> header in m_headers)
            {
                headers[header.Item1] = header.Item2;
            }
            data["headers"] = headers;
            
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("url", out object url))
            {
                m_url = (string)url;
            }

            if (data.TryGetValue("fileName", out object fileName))
            {
                m_fileName = (string)fileName;
            }

            if (data.TryGetValue("method", out object method))
            {
                m_method = (WebRequestMethod)Enum.Parse(typeof(WebRequestMethod), (string)method);
            }
            
            if (data.TryGetValue("headers", out object headers))
            {
                Dictionary<string, object> headerData = (Dictionary<string, object>)headers;
                m_headers = headerData.Select(a => new Tuple<string, string>(a.Key, (string)a.Value)).ToList();
            }
        }
    }
}