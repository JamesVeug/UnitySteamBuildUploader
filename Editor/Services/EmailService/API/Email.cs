using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Thin static wrapper around <see cref="EmailWrapper"/> that pulls the SMTP
    /// server, From identity and credentials from a selected
    /// <see cref="EmailConfig.EmailAccount"/>. Accounts are managed under
    /// Edit -> Preferences -> Build Uploader -> Services -> Email (credentials)
    /// and Project Settings -> Build Uploader -> Services -> Email (server / from).
    ///
    /// The <see cref="Enabled"/> flag is stored in <see cref="ProjectEditorPrefs"/>
    /// so it stays scoped to this project on this machine.
    /// </summary>
    public static partial class Email
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("email_enabled", false);
            set => ProjectEditorPrefs.SetBool("email_enabled", value);
        }

        /// <summary>
        /// Send an email using the SMTP configuration on <paramref name="account"/>.
        /// Returns true on success and forwards the SMTP exception text to
        /// <paramref name="result"/> on failure.
        /// </summary>
        public static async Task<bool> SendEmail(
            EmailConfig.EmailAccount account,
            string toEmail,
            string subject,
            string body,
            List<string> ccEmails = null,
            List<string> bccEmails = null,
            List<string> attachmentFiles = null,
            UploadTaskReport.StepResult result = null)
        {
            if (account == null)
            {
                result?.SetFailed("Failed to send email: no account provided.");
                return false;
            }

            EmailWrapper mail = new EmailWrapper
            {
                Host = account.Host,
                Port = account.Port,
                FromEmail = account.FromEmail,
                FromDisplayName = account.FromDisplayName,
                CredentialEmail = account.CredentialEmail,
                CredentialPassword = account.CredentialPassword,
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
            };

            if (ccEmails != null)
            {
                mail.CCEmails.AddRange(ccEmails);
            }

            if (bccEmails != null)
            {
                mail.BBEmails.AddRange(bccEmails);
            }

            if (attachmentFiles != null)
            {
                mail.AttachmentFiles.AddRange(attachmentFiles);
            }

            EmailWrapper.Result response = await mail.SendEmail();
            if (!response.Successful)
            {
                result?.SetFailed($"Failed to send email: {response.Response}");
                return false;
            }

            result?.AddLog($"Email sent to {toEmail} via account '{account.Name}'");
            return true;
        }
    }
}
