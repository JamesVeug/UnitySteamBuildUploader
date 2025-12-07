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
            
            bool modified = false;

            string key = command.Key;
            if (key.Length > 0)
            {
                if (key[0] == '{')
                {
                    key = key.Substring(1, key.Length - 2);
                }
            }

            if (key.Length > 0)
            {
                if (key[key.Length - 1] == '}')
                {
                    key = key.Substring(0, key.Length - 2);
                }
            }

            if (EditorUtils.FormatStringTextArea(ref key, ref showFormatted, m_context))
            {
                if (key.Length > 0)
                {
                    if (key[0] != '{')
                    {
                        key = "{" + key;
                    }
                    if (key[key.Length - 1] != '}')
                    {
                        key += "}";
                    }
                }
                command.Key = key;
                modified = true;
            }

            return modified;
        }
    }
}