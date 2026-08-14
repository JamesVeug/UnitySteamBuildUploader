using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static partial class Git
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("git_enabled", false);
            set
            {
                if (Enabled == value)
                {
                    return;
                }

                ProjectEditorPrefs.SetBool("git_enabled", value);
                ResetExecutable();
            }
        }

        public static string ExecutablePath
        {
            get
            {
#if UNITY_EDITOR_WIN
                string defaultPath = "git";
#else
                string defaultPath = "/usr/bin/git";
#endif
                return EditorPrefs.GetString("BuildUploader_GitPath", defaultPath);
            }
            set
            {
                if (ExecutablePath == value)
                {
                    return;
                }

                EditorPrefs.SetString("BuildUploader_GitPath", value);
                ResetExecutable();
            }
        }

        public static string Executable
        {
            get
            {
                ResolveExecutable();
                return m_executable;
            }
        }

        public static string Version
        {
            get
            {
                ResolveExecutable();
                return m_version;
            }
        }

        public static bool IsAvailable => !string.IsNullOrEmpty(Executable);

        internal readonly struct GitSnapshot
        {
            public readonly bool Available;
            public readonly string Branch;
            public readonly string Commit;
            public readonly string CommitShort;
            public readonly string CommitMessage;
            public readonly string CommitMessageSubject;
            public readonly string CommitMessageBody;
            public readonly string CommitAuthor;
            public readonly string CommitAuthorEmail;
            public readonly string CommitDate;
            public readonly string Tag;

            public GitSnapshot(string branch, string commit, string commitShort, string commitMessage,
                string commitMessageSubject, string commitMessageBody, string commitAuthor,
                string commitAuthorEmail, string commitDate, string tag)
            {
                Available = true;
                Branch = branch;
                Commit = commit;
                CommitShort = commitShort;
                CommitMessage = commitMessage;
                CommitMessageSubject = commitMessageSubject;
                CommitMessageBody = commitMessageBody;
                CommitAuthor = commitAuthor;
                CommitAuthorEmail = commitAuthorEmail;
                CommitDate = commitDate;
                Tag = tag;
            }

            private GitSnapshot(bool available)
            {
                Available = available;
                Branch = "";
                Commit = "";
                CommitShort = "";
                CommitMessage = "";
                CommitMessageSubject = "";
                CommitMessageBody = "";
                CommitAuthor = "";
                CommitAuthorEmail = "";
                CommitDate = "";
                Tag = "";
            }

            public static readonly GitSnapshot Unavailable = new GitSnapshot(false);
        }

        private const double RefreshSeconds = 5;

        // ASCII unit separator - git emits it for %x1f and no commit message realistically contains one.
        private const char FieldSeparator = '\u001f';

        private static readonly object m_lock = new object();
        private static GitSnapshot m_snapshot = GitSnapshot.Unavailable;
        private static DateTime m_snapshotTakenUtc = DateTime.MinValue;

        private static string m_projectRoot;
        private static string m_executable = "";
        private static string m_version = "";
        private static bool m_executableResolved;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            m_projectRoot = Path.GetDirectoryName(Application.dataPath);
        }

        public static GitSnapshot GetSnapshot()
        {
            lock (m_lock)
            {
                if ((DateTime.UtcNow - m_snapshotTakenUtc).TotalSeconds < RefreshSeconds)
                {
                    return m_snapshot;
                }

                m_snapshot = TakeSnapshot();
                m_snapshotTakenUtc = DateTime.UtcNow;
                return m_snapshot;
            }
        }

        public static void Invalidate()
        {
            lock (m_lock)
            {
                m_snapshotTakenUtc = DateTime.MinValue;
            }
        }

        private static GitSnapshot TakeSnapshot()
        {
            string git = Executable;
            if (string.IsNullOrEmpty(git))
            {
                return GitSnapshot.Unavailable;
            }

            if (!TryRun(git, "rev-parse --abbrev-ref HEAD", out string branch))
            {
                // We are NOT in a repo with at least one commit
                return GitSnapshot.Unavailable;
            }

            // Fields are separated by %x1f commits can have multiple lines
            TryRun(git, "log -1 --date=format:%Y-%m-%d --pretty=format:%H%x1f%h%x1f%B%x1f%s%x1f%b%x1f%an%x1f%ae%x1f%ad", out string log);
            string[] fields = log.Split(FieldSeparator);

            // Empty when the repo has no tags yet - git describe exits 128 for that, which is not an error
            // worth failing the rest of the snapshot over.
            TryRun(git, "describe --tags --abbrev=0", out string tag);

            return new GitSnapshot(
                branch: branch,
                commit: Field(fields, 0),
                commitShort: Field(fields, 1),
                commitMessage: Field(fields, 2),
                commitMessageSubject: Field(fields, 3),
                commitMessageBody: Field(fields, 4),
                commitAuthor: Field(fields, 5),
                commitAuthorEmail: Field(fields, 6),
                commitDate: Field(fields, 7),
                tag: tag);
        }

        private static string Field(string[] fields, int index)
        {
            return index < fields.Length ? fields[index].Trim() : "";
        }

        private static bool TryRun(string git, string args, out string output)
        {
            ProcessUtils.ProcessResult result = ProcessUtils.RunSync(git, args, m_projectRoot);
            output = result.IsSuccessful ? result.Output.Trim() : "";
            return result.IsSuccessful && !string.IsNullOrEmpty(output);
        }

        public static void ResetExecutable()
        {
            m_executableResolved = false;
            m_executable = "";
            m_version = "";
            Invalidate();
        }

        private static void ResolveExecutable()
        {
            if (m_executableResolved || !Enabled)
            {
                return;
            }

            foreach (string candidate in Candidates())
            {
                ProcessUtils.ProcessResult result = ProcessUtils.RunSync(candidate, "--version", m_projectRoot);
                if (result.IsSuccessful)
                {
                    m_executable = candidate;
                    m_version = result.Output.Trim();
                    break;
                }
            }

            m_executableResolved = true;
        }

        private static IEnumerable<string> Candidates()
        {
            string preferred = ExecutablePath;
            if (!string.IsNullOrEmpty(preferred))
            {
                yield return preferred;
            }

            yield return "git";

#if !UNITY_EDITOR_WIN
            // A Unity launched from Finder or the Dock inherits a minimal PATH that often has no git in it.
            yield return "/usr/bin/git";
            yield return "/usr/local/bin/git";
            yield return "/opt/homebrew/bin/git";
#endif
        }
    }
}
