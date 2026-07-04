using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Runtime counterpart of a <see cref="SetVariableNode"/>. Follows the same pattern the rest of the codebase
    /// uses for "produced now, consumed later" values (see e.g. SlackMessageChannelAction.StringContextModifier.cs):
    /// a single Context command is registered once, at compile time, with a getter that reads
    /// <see cref="CurrentValue"/>; "setting" the variable is just updating that field when this node runs.
    /// Because it goes through <see cref="Context"/>, the value is also usable as a plain {name} token anywhere
    /// Context.FormatString already runs (e.g. a LocalPathDestinationNode's Local Path field) - not just through
    /// an explicit GetVariableNode wire.
    /// </summary>
    public class SetVariableNodeV2 : AUploadNodeV2
    {
        public string CurrentValue { get; private set; } = string.Empty;

        private readonly string m_name;
        private readonly Func<string> m_valueProvider;

        public SetVariableNodeV2(string name, Func<string> valueProvider)
        {
            m_name = name;
            m_valueProvider = valueProvider;
        }

        public override Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.PostUploadActions);

            CurrentValue = m_valueProvider?.Invoke() ?? string.Empty;
            result.AddLog($"SetVariableNode: {m_name} = {CurrentValue}");

            return Task.FromResult(true);
        }
    }
}
