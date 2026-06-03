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
            GUI.Label(r, "Name");
            r.x += r.width;
            r.width = 120;
            string newName = GUI.TextField(r, element.Name);
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            r.x += r.width + padding;

            r.width = labelWidth;
            GUI.Label(r, "Issuer");
            r.x += r.width;
            r.width = Mathf.Max(100, rect.x + rect.width - r.x - labelWidth - 100 - padding * 2);
            string newIssuer = GUI.TextField(r, element.IssuerID);
            if (newIssuer != element.IssuerID)
            {
                element.IssuerID = newIssuer;
                dirty = true;
            }
            r.x += r.width + padding;

            r.width = labelWidth;
            GUI.Label(r, "Key ID");
            r.x += r.width;
            r.width = rect.x + rect.width - r.x;
            string newKey = GUI.TextField(r, element.KeyID);
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
