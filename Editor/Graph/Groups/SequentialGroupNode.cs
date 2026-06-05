#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;

namespace Wireframe
{
    /// <summary>A group whose contained operations run one after another, completing when the last finishes.</summary>
    [Serializable]
    public class SequentialGroupNode : AGroupNode
    {
        public override GroupMode Mode => GroupMode.Sequential;
    }
}
#endif
