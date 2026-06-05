#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>Copy a folder (Source Path) to a Destination Path. Compiles to FolderSource → LocalPathDestination.</summary>
    [Serializable]
    [UseWithContext(typeof(SequentialGroupNode), typeof(ParallelGroupNode))]
    public class CopyFolderBlock : ACopyBlock
    {
        protected override AUploadSource CreateSourceInstance() => new FolderSource();
    }
}
#endif
