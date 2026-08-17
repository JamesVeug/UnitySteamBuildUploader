using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Send an email via SMTP using an account configured under
    /// Edit -> Preferences -> Build Uploader -> Services -> Email
    /// and Project Settings -> Build Uploader -> Services -> Email.
    ///
    /// NOTE: This class's namespace path is saved in the JSON file so avoid renaming.
    /// </summary>
    [Experimental]
    [Wiki(nameof(EmailSendMailAction), "actions", "Send an email via SMTP using one of the accounts configured under Build Uploader -> Services -> Email.")]
    [UploadAction("Email Send Mail")]
    public partial class EmailSendMailAction : AUploadAction
    {
        [Wiki("Account", "Which configured SMTP account will be sending the email.", 1)]
        private EmailConfig.EmailAccount m_account;

        [Wiki("To", "Recipient email address. Supports string formatting.", 2)]
        private string m_to = "";

        [Wiki("Subject", "Subject line of the email. Supports string formatting.", 3)]
        private string m_subject = "";

        [Wiki("Body", "Body of the email. Supports string formatting.", 4)]
        private string m_body = "";

        [Wiki("CC", "Optional list of CC recipient email addresses. Each entry supports string formatting.", 5)]
        private List<string> m_ccEmails = new List<string>();

        [Wiki("BCC", "Optional list of BCC recipient email addresses. Each entry supports string formatting.", 6)]
        private List<string> m_bccEmails = new List<string>();

        [Wiki("Attachments", "Optional list of file paths to attach to the email. Each path supports string formatting so tokens like {sourceFile} resolve at send time.", 7)]
        private List<string> m_attachments = new List<string>();

        public EmailSendMailAction() : base()
        {
            // Required for reflection
        }

        public void SetAccount(EmailConfig.EmailAccount account)
        {
            m_account = account;
        }

        public void SetTo(string to)
        {
            m_to = to;
        }

        public void SetSubject(string subject)
        {
            m_subject = subject;
        }

        public void SetBody(string body)
        {
            m_body = body;
        }

        public void AddCC(string ccEmail)
        {
            m_ccEmails.Add(ccEmail);
        }

        public void AddBCC(string bccEmail)
        {
            m_bccEmails.Add(bccEmail);
        }

        public void AddAttachment(string attachmentPath)
        {
            m_attachments.Add(attachmentPath);
        }

        public override async Task<bool> Execute(UploadTaskReport.StepResult stepResult)
        {
            string to = m_context.FormatString(m_to);
            string subject = m_context.FormatString(m_subject);
            string body = m_context.FormatString(m_body);

            List<string> ccEmails = FormatList(m_ccEmails);
            List<string> bccEmails = FormatList(m_bccEmails);
            List<string> attachments = FormatList(m_attachments);

            return await Email.SendEmail(m_account, to, subject, body, ccEmails, bccEmails, attachments, stepResult);
        }

        private List<string> FormatList(List<string> raw)
        {
            if (raw == null || raw.Count == 0)
            {
                return null;
            }

            List<string> formatted = new List<string>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
            {
                string value = raw[i];
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                formatted.Add(m_context.FormatString(value));
            }

            return formatted;
        }

        public override void TryGetErrors(List<GUIContent> errors)
        {
            base.TryGetErrors(errors);

            EmailService service = InternalUtils.GetService<EmailService>();
            if (!service.IsReadyToStartBuild(out GUIContent reason))
            {
                errors.Add(reason);
            }

            if (m_account == null)
            {
                errors.Add(new GUIContent("Email Account is not set."));
            }
            else
            {
                if (string.IsNullOrEmpty(m_account.Host))
                {
                    errors.Add(service.ProjectSettingsLink($"Email Account '{m_account.Name}' has no SMTP Host configured.", ""));
                }

                if (string.IsNullOrEmpty(m_account.FromEmail))
                {
                    errors.Add(service.ProjectSettingsLink($"Email Account '{m_account.Name}' has no From Email configured.", ""));
                }

                if (string.IsNullOrEmpty(m_account.CredentialEmail))
                {
                    errors.Add(service.PreferencesLink($"Email Account '{m_account.Name}' has no Username set.", ""));
                }

                if (string.IsNullOrEmpty(m_account.CredentialPassword))
                {
                    errors.Add(service.ProjectSettingsLink($"Email Account '{m_account.Name}' has no Password set.", ""));
                }
            }

            if (string.IsNullOrEmpty(m_to))
            {
                errors.Add(new GUIContent("To is not set."));
            }

            if (string.IsNullOrEmpty(m_subject))
            {
                errors.Add(new GUIContent("Subject is not set."));
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "account", m_account?.Id ?? 0 },
                { "to", m_to },
                { "subject", m_subject },
                { "body", m_body },
                { "cc", new List<object>(m_ccEmails) },
                { "bcc", new List<object>(m_bccEmails) },
                { "attachments", new List<object>(m_attachments) },
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            EmailConfig.EmailAccount[] accounts = EmailUIUtils.AccountPopup.Values;
            if (data.TryGetValue("account", out object accountId) && accountId != null)
            {
                m_account = accounts.FirstOrDefault(a => a.Id == (long)accountId);
            }

            m_to = ReadString(data, "to");
            m_subject = ReadString(data, "subject");
            m_body = ReadString(data, "body");
            m_ccEmails = ReadStringList(data, "cc");
            m_bccEmails = ReadStringList(data, "bcc");
            m_attachments = ReadStringList(data, "attachments");
        }

        private static string ReadString(Dictionary<string, object> data, string key)
        {
            if (data.TryGetValue(key, out object value) && value != null)
            {
                return value.ToString();
            }
            return string.Empty;
        }

        private static List<string> ReadStringList(Dictionary<string, object> data, string key)
        {
            List<string> result = new List<string>();
            if (!data.TryGetValue(key, out object value) || value == null)
            {
                return result;
            }

            if (value is List<object> objList)
            {
                foreach (object o in objList)
                {
                    if (o != null)
                    {
                        result.Add(o.ToString());
                    }
                }
            }
            else if (value is List<string> stringList)
            {
                result.AddRange(stringList);
            }

            return result;
        }
    }
}
