using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Edits API keys in Preferences (user scope). Exposes the per-machine .p8 file path
    /// (the secret) plus Issuer / Key ID for convenience.
    ///
    /// InternalReorderableList does not allow per-element heights, so this draws on a
    /// single row matching the Slack reorderable convention.
    /// </summary>
    public class ReorderableListOfAppleApiKeysPreferences : InternalReorderableList<AppleConfig.AppleApiKey>
    {
        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            AppleConfig.AppleApiKey element = list[index];

            float labelWidth = 50f;
            float padding = 5f;

            // Name
            Rect r = new Rect(containerRect.x, containerRect.y, labelWidth, containerRect.height);
            GUI.Label(r, new GUIContent("Name", "Friendly name for this API key (e.g. Release CI Key). UI only — not sent to Apple."));
            r.x += r.width;
            r.width = 100;
            string newName = EditorUtils.PlaceholderTextField(r, element.Name, "e.g. Release CI Key");
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            r.x += r.width + padding;

            // Issuer ID
            r.width = labelWidth;
            GUI.Label(r, new GUIContent("Issuer", "App Store Connect Issuer ID (a UUID). Edit -> Users and Access -> Integrations -> App Store Connect API."));
            r.x += r.width;
            r.width = 120;
            string newIssuer = EditorUtils.PlaceholderTextField(r, element.IssuerID, "e.g. xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
            if (newIssuer != element.IssuerID)
            {
                element.IssuerID = newIssuer;
                dirty = true;
            }
            r.x += r.width + padding;

            // Key ID
            r.width = labelWidth;
            GUI.Label(r, new GUIContent("Key ID", "The 10-character Key ID shown next to the generated key in App Store Connect. Also the {KeyID} in AuthKey_{KeyID}.p8."));
            r.x += r.width;
            r.width = 80;
            string newKey = EditorUtils.PlaceholderTextField(r, element.KeyID, "e.g. 119R4HXF34");
            if (newKey != element.KeyID)
            {
                element.KeyID = newKey;
                dirty = true;
            }
            r.x += r.width + padding;

            // .p8 path + Browse — fills the rest of the row
            r.width = labelWidth;
            GUI.Label(r, new GUIContent(".p8 File", "Local path to the AuthKey_{KeyID}.p8 private key file downloaded from App Store Connect. Stored per-machine."));
            r.x += r.width;

            float browseWidth = 60;
            float pathWidth = Mathf.Max(80, containerRect.x + containerRect.width - r.x - browseWidth - padding);
            r.width = pathWidth;
            string currentPath = element.PrivateKeyPath;
            string newPath = EditorUtils.PlaceholderTextField(r, currentPath, "e.g. /Users/me/keys/AuthKey_XXXXXXXXXX.p8");
            if (newPath != currentPath)
            {
                element.PrivateKeyPath = newPath;
                dirty = true;
            }
            r.x += r.width + padding;

            r.width = browseWidth;
            if (GUI.Button(r, "Browse"))
            {
                string startDir = string.IsNullOrEmpty(currentPath) ? "" : System.IO.Path.GetDirectoryName(currentPath);
                string picked = EditorUtility.OpenFilePanel("Select Apple .p8 Private Key", startDir, "p8");
                if (!string.IsNullOrEmpty(picked))
                {
                    element.PrivateKeyPath = picked;
                    dirty = true;
                }
            }
        }

        protected override AppleConfig.AppleApiKey CreateItem(int index)
        {
            return new AppleConfig.AppleApiKey(index, "MyApiKey");
        }

        protected override int CompareTo(AppleConfig.AppleApiKey a, AppleConfig.AppleApiKey b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
