using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Preferences-scope reorderable list of <see cref="EmailConfig.EmailAccount"/>.
    /// Exposes Name, the SMTP username and the per-machine password. The
    /// team-shared SMTP server and From identity are edited in Project Settings
    /// via <see cref="ReorderableListOfEmailAccountsProjectSettings"/>. This
    /// mirrors the Slack pattern where Preferences exposes the secret
    /// (App.Token) and ProjectSettings exposes the shared identity.
    /// </summary>
    public class ReorderableListOfEmailAccountsPreferences : InternalReorderableList<EmailConfig.EmailAccount>
    {
        private bool showPassword;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            EmailConfig.EmailAccount element = list[index];

            const float nameLabelWidth = 50f;
            const float nameWidth = 110f;
            const float usernameLabelWidth = 70f;
            const float usernameWidth = 150f;
            const float passwordLabelWidth = 70f;
            const float showToggleWidth = 55f;
            const float padding = 4f;

            Rect cursor = new Rect(containerRect.x, containerRect.y, nameLabelWidth, containerRect.height);

            GUI.Label(cursor, new GUIContent("Name", "Display name for this email account. UI only — not sent."));
            cursor.x += cursor.width;

            cursor.width = nameWidth;
            string newName = EditorUtils.PlaceholderTextField(cursor, element.Name, "e.g. Release Mailer");
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = usernameLabelWidth;
            GUI.Label(cursor, new GUIContent("Username", "SMTP username. For Gmail this is your full email address."));
            cursor.x += cursor.width;

            cursor.width = usernameWidth;
            string newUsername = EditorUtils.PlaceholderTextField(cursor, element.CredentialEmail ?? "", "e.g. you@gmail.com");
            if (newUsername != element.CredentialEmail)
            {
                element.CredentialEmail = newUsername;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = passwordLabelWidth;
            GUI.Label(cursor, new GUIContent("Password", "SMTP password or App Password. Stored per-machine, per-project."));
            cursor.x += cursor.width;

            float passwordWidth = containerRect.xMax - cursor.x - showToggleWidth - padding;
            cursor.width = Mathf.Max(60f, passwordWidth);
            string currentPassword = element.CredentialPassword;
            string newPassword;
            if (showPassword)
            {
                newPassword = EditorUtils.PlaceholderTextField(cursor, currentPassword, "xxxxxxxxxxxxxxxx");
            }
            else
            {
                newPassword = GUI.PasswordField(cursor, currentPassword, '*');
            }
            if (newPassword != currentPassword)
            {
                element.CredentialPassword = newPassword;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = showToggleWidth;
            showPassword = GUI.Toggle(cursor, showPassword, showPassword ? "Hide" : "Show", GUI.skin.button);
        }

        protected override EmailConfig.EmailAccount CreateItem(int index)
        {
            return new EmailConfig.EmailAccount(index, "MyAccount");
        }

        protected override int CompareTo(EmailConfig.EmailAccount a, EmailConfig.EmailAccount b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
