#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>Copy a single file (Source Path) to a Destination Path. Compiles to FileSource → LocalPathDestination.</summary>
    [Serializable]
    [UseWithContext(typeof(SequentialGroupNode), typeof(ParallelGroupNode))]
    public class CopyFileBlock : ACopyBlock
    {
        protected override AUploadSource CreateSourceInstance() => new FileSource();
    }
}
#endif
