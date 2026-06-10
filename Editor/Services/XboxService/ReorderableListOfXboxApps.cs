using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Project Settings list: Name, Product ID, Tenant ID, and Client ID for each Xbox app.
    /// None of these fields are secrets — all are safe to commit.
    /// The client secret is entered separately in Preferences.
    /// </summary>
    public class ReorderableListOfXboxApps : InternalReorderableList<XboxConfig.XboxApp>
    {
        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            XboxConfig.XboxApp element = list[index];

            float labelW = 70f;
            float fieldW = (containerRect.width - labelW * 4f) / 4f;

            Rect r = new Rect(containerRect.x, containerRect.y, labelW, containerRect.height);

            // Name
            GUI.Label(r, "Name");
            r.x += r.width;
            r.width = fieldW;
            string newName = GUI.TextField(r, element.Name);
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            r.x += r.width;

            // Product ID
            r.width = labelW;
            GUI.Label(r, "Product ID");
            r.x += r.width;
            r.width = fieldW;
            string newProductId = GUI.TextField(r, element.ProductId);
            if (newProductId != element.ProductId)
            {
                element.ProductId = newProductId;
                dirty = true;
            }
            r.x += r.width;

            // Tenant ID
            r.width = labelW;
            GUI.Label(r, "Tenant ID");
            r.x += r.width;
            r.width = fieldW;
            string newTenantId = GUI.TextField(r, element.TenantId);
            if (newTenantId != element.TenantId)
            {
                element.TenantId = newTenantId;
                dirty = true;
            }
            r.x += r.width;

            // Client ID
            r.width = labelW;
            GUI.Label(r, "Client ID");
            r.x += r.width;
            r.width = fieldW;
            string newClientId = GUI.TextField(r, element.ClientId);
            if (newClientId != element.ClientId)
            {
                element.ClientId = newClientId;
                dirty = true;
            }
        }

        protected override XboxConfig.XboxApp CreateItem(int index)
        {
            return new XboxConfig.XboxApp(index, "My Xbox App");
        }

        protected override int CompareTo(XboxConfig.XboxApp a, XboxConfig.XboxApp b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
