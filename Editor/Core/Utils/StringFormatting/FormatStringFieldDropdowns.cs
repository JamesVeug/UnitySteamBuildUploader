using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Bridges the stateless format-string text field helpers to the stateful
    /// <see cref="FormatStringAutocompleteTextArea"/> widget so every field gets an autocomplete
    /// dropdown without each call site owning an instance.
    ///
    /// A host window pumps the dropdown by calling <see cref="BeginHost"/> at the very start of its
    /// OnGUI (before any control) and <see cref="EndHost"/> at the very end (outside every layout
    /// scope / scroll view). Fields are drawn through <see cref="Draw"/>, which pools one widget per
    /// control id and tracks the single open dropdown so only its owning host pumps it.
    /// </summary>
    public static class FormatStringFieldDropdowns
    {
        private static readonly Dictionary<int, FormatStringAutocompleteTextArea> s_pool =
            new Dictionary<int, FormatStringAutocompleteTextArea>();

        private static FormatStringAutocompleteTextArea s_active;
        private static object s_activeHost;
        private static object s_currentHost;

        /// <summary>True while a suggestion dropdown is open anywhere. Use it to disable controls
        /// (e.g. an Upload button) that a dropdown may overlap.</summary>
        public static bool IsDropdownOpen => s_active != null && s_active.IsDropdownOpen;

        /// <summary>Call at the very start of a host's OnGUI, before any control is drawn.</summary>
        public static void BeginHost(object host)
        {
            s_currentHost = host;
            if (s_active != null && Equals(s_activeHost, host))
            {
                s_active.HandleOverlayInput();
            }
        }

        /// <summary>Call at the very end of a host's OnGUI, outside every layout scope.</summary>
        public static void EndHost(object host)
        {
            if (s_active != null && Equals(s_activeHost, host))
            {
                s_active.DrawDropdown();
            }

            if (Equals(s_currentHost, host))
            {
                s_currentHost = null;
            }
        }

        /// <summary>
        /// Draws a format-string editable field with autocomplete. <paramref name="id"/> must be a
        /// stable-per-field control id (e.g. GUIUtility.GetControlID). Returns the (possibly edited)
        /// text. Falls back to a plain field when there is no active host or no context.
        /// </summary>
        public static string Draw(int id, string text, Context ctx, bool singleLine, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!s_pool.TryGetValue(id, out FormatStringAutocompleteTextArea widget))
            {
                widget = new FormatStringAutocompleteTextArea("FSAT" + id);
                s_pool[id] = widget;
            }

            bool armed = s_currentHost != null && ctx != null;
            string result = widget.OnGUI(text, ctx, singleLine, armed, style, options);

            if (armed && widget.IsDropdownOpen)
            {
                s_active = widget;
                s_activeHost = s_currentHost;
            }
            else if (s_active == widget)
            {
                s_active = null;
                s_activeHost = null;
            }

            return result;
        }
    }
}
