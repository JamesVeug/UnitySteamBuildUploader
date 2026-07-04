using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Base for nodes that fan out into 1 or more independent branches, each the start of its own chain of nodes.
    /// Concrete subclasses (<see cref="SequentialGroupNode"/>/<see cref="ParallelGroupNode"/>) decide how those
    /// branches are scheduled by <see cref="BuildUploaderGraphCompiler"/> - this base only defines the shape:
    /// a "Branch Count" option and that many numbered output ports.
    ///
    /// Execution continues out through the shared "Execution" port (see <see cref="ABuildUploaderNode"/>) only
    /// once every branch has finished.
    /// </summary>
    [Serializable]
    public abstract class AGroupNode : ABuildUploaderNode
    {
        public const string BRANCH_COUNT_OPTION_NAME = "BranchCount";
        public const string BRANCH_PORT_PREFIX = "Branch";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(BRANCH_COUNT_OPTION_NAME)
                .WithDisplayName("Branch Count")
                .WithDefaultValue(2)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);

            int branchCount = GetBranchCount();
            for (int i = 0; i < branchCount; i++)
            {
                context.AddOutputPort(BRANCH_PORT_PREFIX + i)
                    .WithDisplayName("Branch " + i)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }

        public int GetBranchCount()
        {
            INodeOption option = GetNodeOptionByName(BRANCH_COUNT_OPTION_NAME);
            if (option != null && option.TryGetValue(out int count))
            {
                return Math.Max(1, count);
            }

            return 2;
        }
    }
}
