using System;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Lists API keys in ProjectSettings (project scope). Editable fields here are the
    /// non-secret bits (Name, Issuer ID, Key ID) so they can be committed alongside the
    /// project. The .p8 file path lives in Preferences (per machine).
    /// </summary>
    public class ReorderableListOfAppleApiKeysProjectSettings : InternalReorderableList<AppleConfig.AppleApiKey>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            AppleConfig.AppleApiKey element = list[index];

            float labelWidth = 50f;
            float padding = 5f;

            Rect r = new Rect(rect.x, rect.y, labelWidth, rect.height);
            GUI.Label(r, new GUIContent("Name", "Display name for this API key (e.g. Release CI Key). UI only — not sent to Apple."));
            r.x += r.width;
            r.width = 120;
            string newName = EditorUtils.PlaceholderTextField(r, element.Name, "e.g. Release CI Key");
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            r.x += r.width + padding;

            r.width = labelWidth;
            GUI.Label(r, new GUIContent("Issuer", "App Store Connect Issuer ID (a UUID). Edit -> Users and Access -> Integrations -> App Store Connect API."));
            r.x += r.width;
            r.width = Mathf.Max(100, rect.x + rect.width - r.x - labelWidth - 200 - padding * 2);
            string newIssuer = EditorUtils.PlaceholderTextField(r, element.IssuerID, "e.g. xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
            if (newIssuer != element.IssuerID)
            {
                element.IssuerID = newIssuer;
                dirty = true;
            }
            r.x += r.width + padding;

            r.width = labelWidth;
            GUI.Label(r, new GUIContent("Key ID", "The 10-character Key ID shown next to the generated key in App Store Connect. Also the {KeyID} in AuthKey_{KeyID}.p8."));
            r.x += r.width;
            r.width = rect.x + rect.width - r.x;
            string newKey = EditorUtils.PlaceholderTextField(r, element.KeyID, "e.g. 2X9R4HXF34");
            if (newKey != element.KeyID)
            {
                element.KeyID = newKey;
                dirty = true;
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
