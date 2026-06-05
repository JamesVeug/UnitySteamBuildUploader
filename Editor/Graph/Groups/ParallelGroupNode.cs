#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;

namespace Wireframe
{
    /// <summary>A group whose contained operations run concurrently, completing when all of them finish.</summary>
    [Serializable]
    public class ParallelGroupNode : AGroupNode
    {
        public override GroupMode Mode => GroupMode.Parallel;
    }
}
#endif
