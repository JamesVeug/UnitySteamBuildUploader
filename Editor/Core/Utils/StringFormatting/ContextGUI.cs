using UnityEngine;

namespace Wireframe
{
    public static class ContextGUI
    {
        public static bool DrawKey(Command command, ref bool showFormatted, Context m_context)
        {
            if (command == null)
            {
                return false;
            }
            
            string name = ToDisplayName(command.Key);
            if (!EditorUtils.FormatStringTextArea(ref name, ref showFormatted, m_context))
            {
                return false;
            }

            command.Key = ToKey(name);
            return true;
        }

        /// <summary>
        /// Strips the surrounding braces so the user edits the bare name. Each brace is removed
        /// independently - trimming a fixed two characters off a key that only has one of them
        /// (eg: "#{buildNumber}") eats a character of the name every time the field is redrawn.
        /// </summary>
        private static string ToDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }

            int start = key[0] == '{' ? 1 : 0;
            int end = key[key.Length - 1] == '}' ? key.Length - 1 : key.Length;
            return start < end ? key.Substring(start, end - start) : "";
        }

        /// <summary>
        /// Wraps the name back into a single '{name}' token. Wrapping unconditionally is what keeps
        /// this the exact inverse of <see cref="ToDisplayName"/> - only adding the braces when the
        /// name doesn't already start/end with one lets a name like "#{buildNumber}" keep its own
        /// braces and collect another pair on every round trip.
        /// </summary>
        private static string ToKey(string name)
        {
            return string.IsNullOrEmpty(name) ? "" : "{" + name + "}";
        }
    }
}