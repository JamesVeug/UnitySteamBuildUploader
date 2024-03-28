using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    public class UnityCloudAPI
    {
        public static List<UnityCloudTarget> CloudBuildTargets { get; private set; } = new List<UnityCloudTarget>();
        public static List<(UnityCloudTarget, List<UnityCloudBuild>)> CurrentBuilds { get; private set; }
        public static List<UnityCloudBuild> AllBuilds { get; private set; }
        public static int TotalBuilds { get; private set; }
        public static bool IsSyncing { get; private set; }
        public static bool HasSynced { get; private set; }
        public static int TotalSyncs { get; private set; }
        public static DateTime LastSyncDateTime { get; private set; }
        public static bool IsInitialized => UnityCloud.Instance.IsInitialized();

        public static List<UnityCloudBuild> GetBuildsForTarget(UnityCloudTarget target)
        {
            if (CurrentBuilds == null)
            {
                return null;
            }

            for (int i = 0; i < CurrentBuilds.Count; i++)
            {
                if (CurrentBuilds[i].Item1 == target)
                {
                    return CurrentBuilds[i].Item2;
                }
            }

            return null;
        }

        public static UnityCloudTarget GetTargetForBuild(UnityCloudBuild build)
        {
            string buildtargetid = build.buildtargetid;
            for (int i = 0; i < CloudBuildTargets.Count; i++)
            {
                if (CloudBuildTargets[i].buildtargetid == buildtargetid)
                {
                    return CloudBuildTargets[i];
                }
            }

            return null;
        }

        public static void SyncBuilds(Action callback = null)
        {
            IsSyncing = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(SyncCoroutine(callback));
        }

        public static IEnumerator SyncCoroutine(Action callback)
        {
            yield return SyncTargetsCoroutine(null);
            yield return SyncBuildsCoroutine(null);
            callback?.Invoke();
        }

        public static IEnumerator SyncTargetsCoroutine(Action callback)
        {
            IsSyncing = true;
            List<UnityCloudTarget> allTargets = new List<UnityCloudTarget>();

            // Send request
            UnityWebRequest www = GetAllTargets();
            yield return www.SendWebRequest();

            // Wait request
            while (www.isDone == false)
                yield return null;

            string downloadHandlerText = www.downloadHandler.text;
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.LogError("Could not sync builds with UnityCloud. Have you filled in the settings tab?:\nError: " +
                               downloadHandlerText);
                if (downloadHandlerText.Contains("Rate limit exceeded"))
                {
                    yield return new WaitForSeconds(10);
                }
                else if (downloadHandlerText.Contains("Not authorized"))
                {
                    yield return new WaitForSeconds(60);
                }
                else
                {
                    yield return new WaitForSeconds(1);
                }

                IsSyncing = false;
                yield break;
            }

            // Populate list
            List<UnityCloudTarget> downloadedBuilds = JsonConvert.DeserializeObject<List<UnityCloudTarget>>(downloadHandlerText);

            allTargets.AddRange(downloadedBuilds);
            //Debug.Log(targets[0].Item1.Name + " has " + b.Count + " builds.");


            allTargets.Sort(((a, b) => b.enabled.CompareTo(a.enabled)));
            CloudBuildTargets = allTargets;
            UnityCloudAPIEditorUtil.TargetPopup.Refresh();

            callback?.Invoke();

            LastSyncDateTime = DateTime.UtcNow;
            HasSynced = true;
            IsSyncing = false;
        }

        public static IEnumerator SyncBuildsCoroutine(Action callback)
        {
            IsSyncing = true;
            List<UnityCloudBuild> allBuilds = new List<UnityCloudBuild>();
            List<(UnityCloudTarget, List<UnityCloudBuild>)> targets =
                new List<(UnityCloudTarget, List<UnityCloudBuild>)>();
            for (int i = 0; i < CloudBuildTargets.Count; i++)
            {
                targets.Add((CloudBuildTargets[i], new List<UnityCloudBuild>()));
            }

            List<UnityWebRequestAsyncOperation> requests = new List<UnityWebRequestAsyncOperation>();

            // Send all requests
            for (int i = 0; i < targets.Count; i++)
            {
                UnityCloudTarget target = targets[i].Item1;
                UnityWebRequest www = GetAllBuilds(target, false);
                //Debug.Log("Fetching builds for: " + target.Name);
                requests.Add(www.SendWebRequest());
            }

            // Wait for all requests to finish
            for (int i = 0; i < requests.Count; i++)
            {
                while (!requests[i].isDone)
                {
                    yield return null;
                }
            }

            // Wait for all requests to come back
            int totalBuilds = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                List<UnityCloudBuild> builds = targets[i].Item2;
                UnityWebRequest www = requests[i].webRequest;
                while (www.isDone == false)
                    yield return null;

                string downloadHandlerText = www.downloadHandler.text;
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.LogError(
                        "Could not sync builds with UnityCloud. Have you filled in the settings tab?:\nError: " +
                        downloadHandlerText);
                    if (downloadHandlerText.Contains("Rate limit exceeded"))
                    {
                        yield return new WaitForSeconds(10);
                    }
                    else if (downloadHandlerText.Contains("Not authorized"))
                    {
                        yield return new WaitForSeconds(60);
                    }
                    else
                    {
                        yield return new WaitForSeconds(1);
                    }

                    IsSyncing = false;
                    yield break;
                }

                // Populate list
                List<UnityCloudBuild> downloadedBuilds =
                    JsonConvert.DeserializeObject<List<UnityCloudBuild>>(downloadHandlerText);

                builds.Clear();
                builds.AddRange(downloadedBuilds);
                builds.Sort(((a, b) => b.build - a.build));

                allBuilds.AddRange(downloadedBuilds);
                totalBuilds += downloadedBuilds.Count;
                //Debug.Log(targets[0].Item1.Name + " has " + b.Count + " builds.");
            }


            allBuilds.Sort(((a, b) => b.CreatedDateTime.CompareTo(a.CreatedDateTime)));
            AllBuilds = allBuilds;
            CurrentBuilds = targets;
            UnityCloudAPIEditorUtil.BuildPopup.Refresh();

            TotalSyncs++;
            TotalBuilds = totalBuilds;
            callback?.Invoke();

            LastSyncDateTime = DateTime.UtcNow;
            HasSynced = true;
            IsSyncing = false;
        }

        private static UnityWebRequest GetAllTargets()
        {
            // org = "myorg"
            // api_key = "Basic xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            // project id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            string url = "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets";
            string parsed = string.Format(url, UnityCloud.Instance.Organization, UnityCloud.Instance.Project);
            UnityWebRequest www = UnityWebRequest.Get(parsed);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Basic " + UnityCloud.Instance.Secret);

            return www;
        }

        private static UnityWebRequest GetAllBuilds(UnityCloudTarget target, bool onlySuccessful = true)
        {
            // org = "myorg"
            // api_key = "Basic xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            // project id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            string url =
                "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets/{2}/builds";
            if (onlySuccessful)
            {
                url += "?buildStatus=success";
            }

            string parsed = string.Format(url, UnityCloud.Instance.Organization, UnityCloud.Instance.Project,
                target.buildtargetid);
            UnityWebRequest www = UnityWebRequest.Get(parsed);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Basic " + UnityCloud.Instance.Secret);

            return www;
        }

        public static IEnumerator DownloadBuildArtifacts(UnityCloudBuild build, string directory)
        {
            List<UnityCloudBuild.Artifact> artifacts = build.GetAllArtifacts();

            List<UnityWebRequestAsyncOperation> operations = new List<UnityWebRequestAsyncOperation>();
            List<string> directories = new List<string>();
            for (int i = 0; i < artifacts.Count; i++)
            {
                for (int j = 0; j < artifacts[i].files.Count; j++)
                {
                    UnityCloudBuild.ArtifactBuild artifactBuild = artifacts[i].files[j];
                    UnityWebRequest request = UnityWebRequest.Get(artifactBuild.href);
                    operations.Add(request.SendWebRequest());

                    // directory/game-windows-development-71.zip
                    string buildTarget = build.buildtargetid + "-" + build.build;
                    Debug.Log("Downloading " + buildTarget);
                    string d = Path.Combine(directory, buildTarget) + ".zip";
                    directories.Add(d);
                }
            }

            for (int i = 0; i < operations.Count; i++)
            {
                yield return operations[i];
                while (!operations[i].isDone)
                {
                    yield return null;
                }

                // Save to disk
                string path = directories[i];
                Debug.Log("Saving to " + path);
                File.WriteAllBytes(path, operations[i].webRequest.downloadHandler.data);
            }
        }

        public static UnityWebRequest StartBuild(string buildTargetID)
        {
            // org = "myorg"
            // api_key = "Basic xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            // project id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            string url = "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets/{2}/builds";

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["clean"] = false;
            data["delay"] = 0;

            string urlParsed = string.Format(url, UnityCloud.Instance.Organization, UnityCloud.Instance.Project,
                buildTargetID);
            string payload = JsonConvert.SerializeObject(data);

            UnityWebRequest www = new UnityWebRequest(urlParsed, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Basic " + UnityCloud.Instance.Secret);

            return www;
        }

        public static UnityWebRequest CancelBuild(string buildTargetID, int buildNumber)
        {
            // org = "myorg"
            // api_key = "Basic xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            // project id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            string url = "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets/{2}/builds/{3}";

            string urlParsed = string.Format(url, UnityCloud.Instance.Organization, UnityCloud.Instance.Project,
                buildTargetID, buildNumber);
            UnityWebRequest www = UnityWebRequest.Delete(urlParsed);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Basic " + UnityCloud.Instance.Secret);

            return www;
        }
    }
}