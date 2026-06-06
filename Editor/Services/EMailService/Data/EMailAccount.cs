using System;

namespace Wireframe
{
    public partial class EmailConfig
    {
        /// <summary>
        /// A single SMTP account that can be used to send an email.
        ///
        /// Team-shared fields (server, From details) live in the JSON config so
        /// they can be checked into source control with the project. The
        /// per-machine credentials (<see cref="CredentialEmail"/> and
        /// <see cref="CredentialPassword"/>) are stored via
        /// <see cref="EncodedEditorPrefs"/> using a project-prefixed key derived
        /// from the account <see cref="Name"/>. That keeps them scoped to this
        /// project on this machine and out of any JSON file that might be
        /// committed.
        ///
        /// NOTE: Renaming an account orphans its stored credentials. The user
        /// will need to re-enter them under the new name. This matches how
        /// sibling services (Slack, Discord) behave with their per-name tokens.
        /// </summary>
        [Serializable]
        public class EmailAccount : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public string Host;
            public int Port;
            public string FromEmail;
            public string FromDisplayName;

            private int m_id;

            private string CredentialEmailKey => ProjectEditorPrefs.ProjectID + "EmailAccountUsername_" + Name;
            public string CredentialEmail
            {
                get => EncodedEditorPrefs.GetString(CredentialEmailKey, "");
                set => EncodedEditorPrefs.SetString(CredentialEmailKey, value);
            }

            private string CredentialPasswordKey => ProjectEditorPrefs.ProjectID + "EmailAccountPassword_" + Name;
            public string CredentialPassword
            {
                get => EncodedEditorPrefs.GetString(CredentialPasswordKey, "");
                set => EncodedEditorPrefs.SetString(CredentialPasswordKey, value);
            }

            public EmailAccount()
            {
                m_id = 0;
                Name = "Template";
                Host = "smtp.gmail.com";
                Port = 587;
                FromEmail = "";
                FromDisplayName = "";
            }

            public EmailAccount(int id, string displayName)
            {
                m_id = id;
                Name = displayName;
                Host = "smtp.gmail.com";
                Port = 587;
                FromEmail = "";
                FromDisplayName = "";
            }
        }
    }
}
