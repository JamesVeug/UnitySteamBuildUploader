using System;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
	public partial class SteamSDK
	{
		// SteamCMD self-updates the first time it runs which can take a long while on a slow connection.
		private const int ProbeTimeoutMs = 120000;

		private const string LoginStatusKey = "steambuild_LoginStatus";
		private const string LoginStatusUserKey = "steambuild_LoginStatusUser";

		/// <summary>
		/// The last known result of logging into Steam as UserName on this machine.
		/// Returns Unknown when we have never checked or when the username has changed since we did.
		/// </summary>
		public static AuthStatus CachedStatus
		{
			get
			{
				string userName = UserName;
				if (string.IsNullOrEmpty(userName))
				{
					return AuthStatus.Unknown;
				}

				// The status only means anything for the account it was recorded against.
				if (!string.Equals(ProjectEditorPrefs.GetString(LoginStatusUserKey), userName, StringComparison.Ordinal))
				{
					return AuthStatus.Unknown;
				}

				return (AuthStatus)ProjectEditorPrefs.GetInt(LoginStatusKey);
			}
			set
			{
				ProjectEditorPrefs.SetString(LoginStatusUserKey, UserName);
				ProjectEditorPrefs.SetInt(LoginStatusKey, (int)value);
			}
		}

		/// <summary>
		/// Why the last probe came back the way it did. Empty until ProbeLoginAsync has run.
		/// </summary>
		public static string LastProbeMessage { get; private set; } = "";

		public static void InvalidateLoginStatus()
		{
			ProjectEditorPrefs.DeleteKey(LoginStatusKey);
			ProjectEditorPrefs.DeleteKey(LoginStatusUserKey);
			LastProbeMessage = "";
			m_credentialsCheckedUtc = DateTime.MinValue;
		}

		/// <summary>
		/// The status to show in the UI without running anything.
		/// Falls back to the offline credential check when we have never probed.
		/// </summary>
		public static AuthStatus GetDisplayStatus()
		{
			AuthStatus status = CachedStatus;
			if (status != AuthStatus.Unknown)
			{
				return status;
			}

			return HasCachedCredentialsCached(UserName) ? AuthStatus.CredentialsCached : AuthStatus.Unknown;
		}

		private const double CredentialsRefreshSeconds = 5;

		private static string m_credentialsCheckedUser;
		private static DateTime m_credentialsCheckedUtc = DateTime.MinValue;
		private static bool m_credentialsFound;

		/// <summary>
		/// HasCachedCredentials touches the disk, and this is called from every repaint of both the
		/// preferences page and the upload window, so only look again every so often.
		/// </summary>
		private static bool HasCachedCredentialsCached(string userName)
		{
			if (string.Equals(m_credentialsCheckedUser, userName, StringComparison.Ordinal) &&
				(DateTime.UtcNow - m_credentialsCheckedUtc).TotalSeconds < CredentialsRefreshSeconds)
			{
				return m_credentialsFound;
			}

			m_credentialsFound = HasCachedCredentials(userName);
			m_credentialsCheckedUser = userName;
			m_credentialsCheckedUtc = DateTime.UtcNow;
			return m_credentialsFound;
		}

		/// <summary>
		/// Cheap enough to call every repaint. Looks for credentials SteamCMD wrote for this user on this machine.
		/// A miss only means we found nothing - never that the login is known to be broken.
		/// </summary>
		public static bool HasCachedCredentials(string userName)
		{
			if (string.IsNullOrEmpty(userName))
			{
				return false;
			}

			string configPath = GetSteamCMDConfigPath();
			if (string.IsNullOrEmpty(configPath))
			{
				return false;
			}

			// SteamCMD writes the account under InstallConfigStore/Software/Valve/Steam/Accounts and a
			// ConnectCache entry beside it. A substring scan is enough - we only need to know the name is in there.
			string configFile = Path.Combine(configPath, "config.vdf");
			if (!File.Exists(configFile))
			{
				return false;
			}

			try
			{
				string contents = File.ReadAllText(configFile);
				if (contents.IndexOf("\"" + userName + "\"", StringComparison.OrdinalIgnoreCase) < 0)
				{
					return false;
				}

				// The sentry file is what actually makes the machine authorized.
				return Directory.GetFiles(configPath, "ssfn*").Length > 0;
			}
			catch (Exception)
			{
				// Locked or unreadable - we simply do not know.
				return false;
			}
		}

		/// <summary>
		/// Runs SteamCMD and asks it to log in as UserName without uploading anything.
		/// Updates CachedStatus unless the result was inconclusive (rate limited, could not run).
		/// </summary>
		public static async Task<AuthStatus> ProbeLoginAsync()
		{
			string userName = UserName;
			if (string.IsNullOrEmpty(userName) || !Instance.IsInitialized)
			{
				LastProbeMessage = "Set a Steam username and a valid SteamSDK path first.";
				return AuthStatus.Unknown;
			}

			string exePath = SteamSDKEXEPath;
			string workingDirectory = Path.GetDirectoryName(exePath);
			string arguments = string.Format("+login \"{0}\" +quit", userName);

			// SteamCMD fails if more than one instance runs at once, so share the upload lock.
			await m_lock.WaitAsync();

			ProcessUtils.ProcessResult result;
			try
			{
				result = await Task.Run(() => ProcessUtils.RunSync(exePath, arguments, workingDirectory,
					ProbeTimeoutMs, null, true));
			}
			finally
			{
				m_lock.Release();
			}

			AuthStatus status = ClassifyLoginOutput(result, out string message);
			LastProbeMessage = message;
			if (status != AuthStatus.Unknown)
			{
				CachedStatus = status;
			}

			return status;
		}

		private static AuthStatus ClassifyLoginOutput(ProcessUtils.ProcessResult result, out string message)
		{
			// SteamCMD exits non-zero on a refused login but explains itself on stdout, so read both.
			string output = result.Output + "\n" + result.Errors;
			if (string.IsNullOrWhiteSpace(output))
			{
				message = string.IsNullOrEmpty(result.Errors)
					? "SteamCMD produced no output."
					: result.Errors;
				return AuthStatus.Unknown;
			}

			// Being rate limited says nothing about whether the machine is authorized, so do not record it.
			if (output.IndexOf("Rate Limit Exceeded", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				message = "Steam rate limited the login. Try again later.";
				return AuthStatus.Unknown;
			}

			string[] lines = output.Split('\n');
			if (Instance.ContainsText(lines, "Logging in user", "OK", out int _))
			{
				message = "";
				return AuthStatus.Authorized;
			}

			if (output.IndexOf("Invalid Password", StringComparison.OrdinalIgnoreCase) >= 0 ||
				output.IndexOf("FAILED login", StringComparison.OrdinalIgnoreCase) >= 0 ||
				output.IndexOf("Steam Guard", StringComparison.OrdinalIgnoreCase) >= 0 ||
				output.IndexOf("password:", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				message = "SteamCMD asked for credentials. This machine is not authorized for " + UserName + ".";
				return AuthStatus.RequiresLogin;
			}

			// A timeout kill or a crash lands here. SteamCMD not saying why is not proof the login is bad -
			// the first run can spend a long time self-updating - so don't record anything.
			message = result.IsSuccessful
				? "Could not tell from the SteamCMD output whether the login worked."
				: "SteamCMD did not complete: " + result.Errors;
			return AuthStatus.Unknown;
		}

		private static string GetSteamCMDConfigPath()
		{
			string exePath = SteamSDKEXEPath;
			if (string.IsNullOrEmpty(exePath))
			{
				return null;
			}

			string builderDirectory = Path.GetDirectoryName(exePath);
			if (string.IsNullOrEmpty(builderDirectory))
			{
				return null;
			}

			// SteamCMD is portable on Windows and keeps its config next to the exe. On macOS and Linux
			// it can instead use the per-user Steam folder, so check both.
			string localConfig = Path.Combine(builderDirectory, "config");
			if (Directory.Exists(localConfig))
			{
				return localConfig;
			}

			string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			if (string.IsNullOrEmpty(home))
			{
				return null;
			}

			string[] candidates =
			{
				Path.Combine(home, "Library/Application Support/Steam/config"),
				Path.Combine(home, "Steam/config"),
				Path.Combine(home, ".steam/steam/config")
			};

			foreach (string candidate in candidates)
			{
				if (Directory.Exists(candidate))
				{
					return candidate;
				}
			}

			return null;
		}
	}
}
