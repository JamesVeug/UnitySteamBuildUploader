using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
	public class UnityCloudBuild : DropdownElement
	{
		public int Id => build;
		public string DisplayName => CreateBuildName();
		
		public int build;
		public string buildtargetid;
		public string buildTargetName;
		public string buildStatus;
		public string platform;
		public string created;
		public string finished; // //2021-06-03T12:21:13.000Z
		public string buildStartTime;
		public List<ArtifactChange> changeset;
		public Dictionary<string, object> links;

		// #####################

		public bool IsSuccessful => buildStatus == "success";
		public bool IsFinished => finished != null;
		public bool HasArtifacts => GetAllArtifacts() != null && GetAllArtifacts().Count > 0;
		public DateTime CreatedDateTime => ConvertStringToDatetime(created);
		public DateTime FinishedDateTime => ConvertStringToDatetime(finished);
		public DateTime BuildStartDateTime => ConvertStringToDatetime(buildStartTime);
		public List<ArtifactChange> GitChangeLogs => changeset;

		// #####################
		// #####################
		// #####################

		private List<Artifact> m_artifacts = null;

		// #####################
		// #####################
		// #####################

		public class ArtifactChangeAuthor
		{
			public string fullName;
			public string absoluteUrl;
		}

		[Serializable]
		public class ArtifactChange
		{
			public DateTime DateTime => DateTime.Parse(timestamp);

			public string commitId;
			public string message;
			public string timestamp;
			public string _id;
			public ArtifactChangeAuthor author;
			public int numAffectedFiles;
		}

		[Serializable]
		public class ArtifactBuild
		{
			public string filename;
			public long size;
			public bool resumable;
			public string md5sum;
			public string href;
		}

		[Serializable]
		public class Artifact
		{
			public string key;
			public string name;
			public bool primary;
			public bool show_download;
			public List<ArtifactBuild> files;

			public bool IsAddressableArtifact()
			{
				return key == "addressable_content";
			}
		}

		// #####################
		// #####################
		// #####################

		public string GetGameArtifactDownloadUrl()
		{
			List<Artifact> artifactList = GetAllArtifacts();
			if (artifactList == null || artifactList.Count == 0)
			{
				//Debug.LogError("Build {0}: Could not get artifact link. No artifacts found!", build);
				return null;
			}

			for (int i = 0; i < artifactList.Count; i++)
			{
				if (!artifactList[i].IsAddressableArtifact())
				{
					return artifactList[i].files[0].href;
				}
			}

			//Debug.LogError("Build {0}: Could not get download link. Does not contain a build in the artifact list!", build);
			return null;
		}

		public string CreateBuildName()
		{
			string parsedPlatform = platform;
			if (platform != null)
			{
				string os = platform.Contains("windows") ? "Windows" : "Mac";
				parsedPlatform = os;
			}

			string target = buildTargetName;
			if (target.ToLower().Contains("development"))
			{
				target = "Dev";
			}
			else
			{
				target = "Release";
			}

			// Windows #44 Release
			return string.Format("{0} #{1} {2}", parsedPlatform, build, target);
		}

		public string GetAddressableArtifactDownloadUrl()
		{
			List<Artifact> artifactList = GetAllArtifacts();
			if (artifactList == null)
			{
				Debug.LogErrorFormat("Build {0}: Could not get artifact link. No artifacts found!", build);
				return null;
			}

			for (int i = 0; i < artifactList.Count; i++)
			{
				if (artifactList[i].IsAddressableArtifact())
				{
					return artifactList[i].files[0].href;
				}
			}

			Debug.LogErrorFormat(
				"Build {0}: Could not get download link. Does not contain a build in the artifact list!", build);
			return null;
		}

		public List<Artifact> GetAllArtifacts()
		{
			if (m_artifacts != null)
			{
				return m_artifacts;
			}

			if (links == null)
			{
				Debug.LogError("Build {0}: Links is null!");
				return null;
			}

			object artifactData;
			if (!links.TryGetValue("artifacts", out artifactData))
			{
				Debug.LogErrorFormat("Build {0}: Could not get artifacts from links!", build);
				return null;
			}

			List<Artifact> artifactList = (List<Artifact>)artifactData;
			m_artifacts = artifactList;
			return m_artifacts;
		}

		private DateTime ConvertStringToDatetime(string s)
		{
			// because CBF using helper methods
			//2021-06-03T12:21:13.000Z
			if (s == null)
			{
				return DateTime.MinValue;
			}

			string[] split = s.Split('T');
			string[] date = split[0].Split('-');
			string[] time = split[1].Split(':');

			var year = int.Parse(date[0]);
			var month = int.Parse(date[1]);
			var day = int.Parse(date[2]);

			var hour = int.Parse(time[0]);
			var minute = int.Parse(time[1]);
			var second = int.Parse(time[2].Substring(0, time[2].IndexOf('.')));

			DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
			return dateTime;
		}
	}
}