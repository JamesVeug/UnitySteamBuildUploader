﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class URLSource
    {
        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                WebRequestMethod newMethod = (WebRequestMethod)EditorGUILayout.EnumPopup(m_method, GUILayout.Width(80));
                if (m_method != newMethod)
                {
                    m_method = newMethod;
                    isDirty = true;
                }

                float width = maxWidth - 120;
                string truncatedText = Utils.TruncateText(m_url, width, "No URL entered...");
                GUILayout.Label(truncatedText, GUILayout.Width(width));
            }
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("URL:", GUILayout.Width(120));
                string newUrl = GUILayout.TextField(m_url);
                if (m_url != newUrl)
                {
                    m_url = newUrl;
                    isDirty = true;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Method:", GUILayout.Width(120));
                WebRequestMethod newMethod = (WebRequestMethod)EditorGUILayout.EnumPopup(m_method);
                if (m_method != newMethod)
                {
                    m_method = newMethod;
                    isDirty = true;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("File Name:", GUILayout.Width(120));
                string newFileName = GUILayout.TextField(m_fileName);
                if (m_fileName != newFileName)
                {
                    m_fileName = newFileName;
                    isDirty = true;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Headers:", GUILayout.Width(120));
                if (m_headers.Count > 0)
                {
                    isDirty |= DrawHeader(m_headers, 0);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("", GUILayout.Width(120));
                using (new EditorGUILayout.VerticalScope())
                {
                    for (var i = 1; i < m_headers.Count; i++)
                    {
                        DrawHeader(m_headers, i);
                    }

                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        m_headers.Add(new Tuple<string, string>("", ""));
                        isDirty = true;
                    }
                }
            }
        }

        private bool DrawHeader(List<Tuple<string, string>> headers, int index)
        {
            var header = headers[index];
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    m_headers.RemoveAt(index);
                    return true;
                }

                string newKey = GUILayout.TextField(header.Item1, GUILayout.Width(120));
                if (header.Item1 != newKey)
                {
                    header = new Tuple<string, string>(newKey, header.Item2);
                    m_headers[index] = header;
                    return true;
                }

                string newValue = GUILayout.TextField(header.Item2);
                if (header.Item2 != newValue)
                {
                    header = new Tuple<string, string>(header.Item1, newValue);
                    m_headers[index] = header;
                    return true;
                }
            }

            return false;
        }
    }
}