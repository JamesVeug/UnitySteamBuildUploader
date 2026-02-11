using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

// TODO: Move requests to a wrapper
#pragma warning disable CS0618 // Type or member is obsolete

namespace Wireframe
{
    /// <summary>
    /// Download something from online
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(URLSource), "sources", "Specify a url to download content from.")]
    [UploadSource("URL", "Download from URL...", true)]
    public partial class URLSource : AUploadSource
    {
        [Wiki("URL", "The URL to download from: eg: https://github.com/JamesVeug/UnitySteamBuildUploader/raw/refs/heads/main/LargeIcon.png")]
        private string m_url;
        
        [Wiki("File Name", "The fileName to download the contents as. eg: MyPicture.png")]
        private string m_fileName;
        
        [Wiki("Method", "The method to use when downloading the file.")]
        private WebRequestMethod m_method;
        
        [Wiki("Headers", "The headers to send with the request.eg:" +
                         "\n  - accept = text/html" +
                         "\n  - accept-language = en-US,en;q=0.9")]
        private List<Tuple<string,string>> m_headers = new List<Tuple<string, string>>();

        private string m_sourcePath;
        
        public URLSource()
        {
            // Required for reflection
        }
        
        public URLSource(string url, WebRequestMethod method=WebRequestMethod.GET)
        {
            SetURL(url, method);
        }
        
        public void SetURL(string url, WebRequestMethod method)
        {
            m_url = url;
            m_method = method;
        }
        
        public void SetHeaders(params Tuple<string,string>[] headers)
        {
            m_headers = headers.ToList();
        }
        
        public void AddHeader(string key, string value)
        {
            m_headers.Add(new Tuple<string, string>(key, value));
        }

        public override async Task<bool> GetSource(bool doNotCache, UploadConfig uploadConfig,
            UploadTaskReport.StepResult stepResult,
            CancellationTokenSource token)
        {
            // Preparing
            string directoryPath = doNotCache ? m_taskContentsFolder : Path.Combine(Preferences.CacheFolderPath, "URLBuilds");
            
            if (!Directory.Exists(directoryPath))
            {
                stepResult.AddLog("Creating directory: " + directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            string fullFilePath = Path.Combine(directoryPath, m_context.FormatString(m_fileName));
            string url = m_context.FormatString(m_url);

            // Only download if we don't have it
            if (!File.Exists(fullFilePath))
            {
                stepResult.AddLog("Downloading from URL: " + url);

                UnityWebRequest request = new UnityWebRequest(url, m_method.ToString());
                foreach (Tuple<string,string> header in m_headers)
                {
                    request.SetRequestHeader(header.Item1, header.Item2);
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();

                
                UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();

                // Wait for it to be downloaded?
                while (!webRequest.isDone)
                {
                    stepResult.SetPercentComplete(webRequest.progress);
                    await Task.Yield();
                }
                
                if (request.isHttpError || request.isNetworkError)
                {
                    string message = $"Could not download build from url {url}:\nError: {request.error}";
                    stepResult.AddError(message);
                    return false;
                }

                // Save
                stepResult.AddLog("Saving content to: " + fullFilePath);
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

            // Record where the game is saved to
            m_sourcePath = fullFilePath;

            stepResult.AddLog("Retrieved URL Build: " + m_sourcePath);
            return true;
        }

        public override Task CleanUp(int configIndex, UploadTaskReport.StepResult stepResult)
        {
            base.CleanUp(configIndex, stepResult);
            if (File.Exists(m_sourcePath) && !Utils.CompareDirectories(m_sourcePath, m_taskContentsFolder))
            {
                stepResult.AddLog("Deleting cached file: " + m_sourcePath);
                File.Delete(m_sourcePath);
            }
            
            return Task.CompletedTask;
        }

        public override string SourceFilePath()
        {
            return m_sourcePath;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (string.IsNullOrEmpty(m_url))
            {
                errors.Add("URL not set");
            }

            if (string.IsNullOrEmpty(m_fileName))
            {
                errors.Add("File name not set");
            }
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