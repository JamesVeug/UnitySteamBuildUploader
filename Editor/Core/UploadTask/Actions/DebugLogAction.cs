using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Logs a message to the console and to the upload report.
    /// Also usable as the runtime counterpart of a DebugLogNode in a BuildUploaderGraph.
    ///
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(DebugLogAction), "actions", "Logs a message to the console. Supports string formatting.")]
    [UploadAction("Debug Log")]
    public partial class DebugLogAction : AUploadAction
    {
        [Wiki("Message", "The message to log. Supports string formatting.")]
        private string m_message = "";

        // Not serialized - only set when a graph wires this action's message to another node's live output
        // (e.g. a source's resolved build path), which can't be known until that node has actually run.
        private readonly Func<string> m_messageProvider;

        public DebugLogAction() : base()
        {
            // Required for reflection
        }

        public DebugLogAction(string message) : base()
        {
            m_message = message;
        }

        public DebugLogAction(Func<string> messageProvider) : base()
        {
            m_messageProvider = messageProvider;
        }

        public override Task<bool> Execute(UploadTaskReport.StepResult stepResult)
        {
            string raw = m_messageProvider != null ? m_messageProvider() : m_message;
            string formatted = m_context.FormatString(raw);
            Debug.Log(formatted);
            stepResult.AddLog(formatted);
            return Task.FromResult(true);
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "message", m_message },
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_message = data.TryGetValue("message", out object value) && value != null ? value.ToString() : "";
        }
    }
}
