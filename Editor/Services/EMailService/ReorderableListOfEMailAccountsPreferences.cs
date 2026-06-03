using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Preferences-scope reorderable list of <see cref="EMailConfig.EMailAccount"/>.
    /// Exposes Name, the SMTP username and the per-machine password. The
    /// team-shared SMTP server and From identity are edited in Project Settings
    /// via <see cref="ReorderableListOfEMailAccountsProjectSettings"/>. This
    /// mirrors the Slack pattern where Preferences exposes the secret
    /// (App.Token) and ProjectSettings exposes the shared identity.
    /// </summary>
    public class ReorderableListOfEMailAccountsPreferences : InternalReorderableList<EMailConfig.EMailAccount>
    {
        private bool showPassword;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            EMailConfig.EMailAccount element = list[index];

            const float nameLabelWidth = 50f;
            const float nameWidth = 110f;
            const float usernameLabelWidth = 70f;
            const float usernameWidth = 150f;
            const float passwordLabelWidth = 70f;
            const float showToggleWidth = 55f;
            const float padding = 4f;

            Rect cursor = new Rect(containerRect.x, containerRect.y, nameLabelWidth, containerRect.height);

            GUI.Label(cursor, "Name");
            cursor.x += cursor.width;

            cursor.width = nameWidth;
            string newName = GUI.TextField(cursor, element.Name);
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
            string newUsername = GUI.TextField(cursor, element.CredentialEmail ?? "");
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
                newPassword = GUI.TextField(cursor, currentPassword);
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

        protected override EMailConfig.EMailAccount CreateItem(int index)
        {
            return new EMailConfig.EMailAccount(index, "MyAccount");
        }

        protected override int CompareTo(EMailConfig.EMailAccount a, EMailConfig.EMailAccount b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
