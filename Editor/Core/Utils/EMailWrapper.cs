using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Wireframe
{
	public class EmailWrapper
	{
		public struct Result
		{
			public bool Successful;
			public string Response;
		}
		
		public string ToEmail;
		public string Subject;
		public string Body;
		
		public List<string> CCEmails = new List<string>();
		public List<string> BBEmails = new List<string>();
		public List<string> AttachmentFiles = new List<string>();
		
		public string FromEmail;
		public string FromDisplayName;
		
		public string CredentialEmail;
		public string CredentialPassword;
		
		public string Host = "smtp.gmail.com";
		public int Port = 587;
		
		public async Task<Result> SendEmail()
		{
			try
			{
				MailAddress from = new MailAddress(FromEmail, FromDisplayName);
				MailAddress to = new MailAddress(ToEmail);

				using (MailMessage message = new MailMessage(from, to))
				{
					message.Subject = Subject;
					message.Body = Body;
					message.IsBodyHtml = false; // Set to true for HTML content
					foreach (string attachmentFile in AttachmentFiles)
					{
						message.Attachments.Add(new Attachment(attachmentFile));
					}
					foreach (string email in CCEmails)
					{
						message.CC.Add(new MailAddress(email, FromDisplayName));
					}
					foreach (string email in BBEmails)
					{
						message.Bcc.Add(new MailAddress(email, FromDisplayName));
					}

					using (SmtpClient smtp = new SmtpClient(Host, Port))
					{
						smtp.Credentials = new NetworkCredential(CredentialEmail, CredentialPassword);
						smtp.EnableSsl = true;
						smtp.Timeout = 10000;
						smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

						await smtp.SendMailAsync(message);
						return new Result { Successful = true };
					}
				}
			}
			catch (Exception ex)
			{
				return new Result { Successful = false, Response = ex.ToString() };
			}
		}
	}
}