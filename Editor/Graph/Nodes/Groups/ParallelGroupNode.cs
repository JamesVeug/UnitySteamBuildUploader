using System;

namespace Wireframe
{
    /// <summary>
    /// Runs every connected branch at the same time and waits for all of them to finish.
    /// Compiles to a <see cref="ParallelNodeV2"/>.
    /// </summary>
    [Serializable]
    public class ParallelGroupNode : AGroupNode
    {
    }
}
