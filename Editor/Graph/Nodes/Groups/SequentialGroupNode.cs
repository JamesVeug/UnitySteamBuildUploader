using System;

namespace Wireframe
{
    /// <summary>
    /// Runs each connected branch one at a time, in branch order (Branch 0, then Branch 1, ...).
    /// Stops as soon as one branch fails. Compiles to a <see cref="SequenceNodeV2"/>.
    /// </summary>
    [Serializable]
    public class SequentialGroupNode : AGroupNode
    {
    }
}
