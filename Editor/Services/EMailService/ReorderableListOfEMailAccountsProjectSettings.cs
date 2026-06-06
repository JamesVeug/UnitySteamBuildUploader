using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Project Settings reorderable list of <see cref="EMailConfig.EMailAccount"/>.
    /// Exposes the team-shared SMTP server details, From identity and the
    /// authentication username. The password is intentionally not editable
    /// here — that lives in Preferences so it never leaks via screen shares
    /// or source control.
    /// </summary>
    public class ReorderableListOfEMailAccountsProjectSettings : InternalReorderableList<EMailConfig.EMailAccount>
    {
        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            EMailConfig.EMailAccount element = list[index];

            const float nameLabelWidth = 50f;
            const float nameWidth = 110f;
            const float hostLabelWidth = 40f;
            const float hostWidth = 130f;
            const float portLabelWidth = 35f;
            const float portWidth = 50f;
            const float fromLabelWidth = 75f;
            const float padding = 4f;
            const float fromNameLabelWidth = 90f;

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

            cursor.width = hostLabelWidth;
            GUI.Label(cursor, new GUIContent("Host", "SMTP server hostname. For Gmail this is smtp.gmail.com."));
            cursor.x += cursor.width;

            cursor.width = hostWidth;
            string newHost = EditorUtils.PlaceholderTextField(cursor, element.Host ?? "", "e.g. smtp.gmail.com");
            if (newHost != element.Host)
            {
                element.Host = newHost;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = portLabelWidth;
            GUI.Label(cursor, new GUIContent("Port", "SMTP port. Commonly 587 (TLS) or 465 (SSL)."));
            cursor.x += cursor.width;

            cursor.width = portWidth;
            int newPort = EditorGUI.IntField(cursor, element.Port);
            if (newPort != element.Port)
            {
                element.Port = newPort;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = fromLabelWidth;
            GUI.Label(cursor, new GUIContent("From Email", "Address the email is sent from. Often the same as the username."));
            cursor.x += cursor.width;

            // Split the rest of the row between FromEmail and FromDisplayName,
            // accounting for the "Name" label sitting between them and the one
            // padding gap that separates FromEmail from that label.
            float remaining = Mathf.Max(60f, containerRect.xMax - cursor.x);
            float fieldsAvailable = Mathf.Max(20f, remaining - fromNameLabelWidth - padding);
            float fromEmailWidth = fieldsAvailable * 0.5f;
            float fromNameWidth = fieldsAvailable - fromEmailWidth;

            cursor.width = fromEmailWidth;
            string newFromEmail = EditorUtils.PlaceholderTextField(cursor, element.FromEmail ?? "", "e.g. builds@studio.com");
            if (newFromEmail != element.FromEmail)
            {
                element.FromEmail = newFromEmail;
                dirty = true;
            }
            cursor.x += cursor.width + padding;

            cursor.width = fromNameLabelWidth;
            GUI.Label(cursor, new GUIContent("Display Name", "Display sender name recipients see instead of the raw address."));
            cursor.x += cursor.width;

            cursor.width = fromNameWidth;
            string newFromName = EditorUtils.PlaceholderTextField(cursor, element.FromDisplayName ?? "", "e.g. Build Bot");
            if (newFromName != element.FromDisplayName)
            {
                element.FromDisplayName = newFromName;
                dirty = true;
            }
        }

        protected override EMailConfig.EMailAccount CreateItem(int index)
        {
            return new EMailConfig.EMailAccount(index, "MyAccount");
        }

        protected override int CompareTo(EMailConfig.EMailAccount a, EMailConfig.EMailAccount b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
