using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Wireframe
{
    public readonly struct RequestResult
    {
        public readonly bool IsSuccessful;
        public readonly string Data;
        public readonly byte[] Bytes;
            
        private RequestResult(bool isSuccessful, string data, byte[] bytes)
        {
            IsSuccessful = isSuccessful;
            Data = data;
            Bytes = bytes;
        }
            
        public static RequestResult Successful(string text, byte[] bytes)
        {
            return new RequestResult(true, text, bytes);
        }
            
        public static RequestResult Failed(string reason)
        {
            return new RequestResult(false, reason, null);
        }
    }
    
    public class RequestWrapper : IDisposable
    {
        private UnityWebRequest www;

        public RequestWrapper(string url, string method)
        {
            www = new UnityWebRequest(url, method);
            www.downloadHandler = new DownloadHandlerBuffer();
        }
        
        ~RequestWrapper()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (www != null)
            {
                www.Dispose();
            }
        }

        public async Task<RequestResult> SendAsync(UploadTaskReport.StepResult result, bool updateProgress = false)
        {
            result?.AddLog("Sending request: " + www.method + " " + www.url);
            
            Stopwatch stopwatch = new Stopwatch();
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!www.isDone)
            {
                if (updateProgress)
                {
                    result?.SetPercentComplete(operation.progress);
                }
                await Task.Yield();
            }
            stopwatch.Stop();
            string duration = $"{stopwatch.ElapsedMilliseconds}ms";
            
            if (!IsSuccessful())
            {
                string downloadHandlerText = www.downloadHandler.text;
                string error = $"Request Failed: {www.responseCode} - {downloadHandlerText} over {duration}";
                result?.AddError(error);
                return RequestResult.Failed(error);
            }
            
            result?.AddLog($"Request successful: {www.method} {www.url} - responseCode = {www.responseCode} over {duration}");
            return RequestResult.Successful(www.downloadHandler.text, www.downloadHandler.data);
        }
        
        public static RequestWrapper Post(string url)
        {
            RequestWrapper requestWrapper = new RequestWrapper(url, "POST");
            return requestWrapper;
        } 
        
        public static RequestWrapper Get(string url)
        {
            RequestWrapper requestWrapper = new RequestWrapper(url, "GET");
            return requestWrapper;
        }
        
        public static RequestWrapper Delete(string url)
        {
            RequestWrapper requestWrapper = new RequestWrapper(url, "DELETE");
            return requestWrapper;
        }

        public void SetRequestHeader(string key, string value)
        {
            www.SetRequestHeader(key, value);
        }

        public void SetHeaders(List<Tuple<string, string>> headers)
        {
            foreach (Tuple<string, string> header in headers)
            {
                www.SetRequestHeader(header.Item1, header.Item2);
            }
        }

        public bool IsSuccessful()
        {
#if UNITY_2022_3_OR_NEWER
            return www.result == UnityWebRequest.Result.Success;
#else
            return !(www.isHttpError || www.isNetworkError)
#endif
        }
        
        public void SetJSONData(string json)
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            SetRequestHeader("Content-Type", "application/json");
        }

        public void SetJSONData(object data)
        {
            SetJSONData(JSON.SerializeObject(data));
        }

        public void SetOctetStreamData(byte[] data)
        {
            www.uploadHandler = new UploadHandlerRaw(data);
            www.SetRequestHeader("Content-Type", "application/octet-stream");
        }
    }
}