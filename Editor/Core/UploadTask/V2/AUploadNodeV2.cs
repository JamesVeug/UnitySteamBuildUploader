using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// A single executable unit inside a <see cref="UploadTaskV2"/> run. Unlike the fixed
    /// <see cref="AUploadTask_Step.StepType"/> pipeline, nodes are composed into an arbitrary tree
    /// (see <see cref="SequenceNodeV2"/> and <see cref="ParallelNodeV2"/>) instead of running in one global order.
    ///
    /// NOTE: Logging still goes through <see cref="UploadTaskReport"/>/<see cref="UploadTaskReport.StepResult"/> -
    /// AUploadSource/AUploadDestination/AUploadAction's Prepare/GetSource/Upload/Execute/CleanUp methods all
    /// require that concrete type, so it can't be swapped out without changing every existing source/destination/
    /// action in the codebase. What UploadTaskV2 actually frees up is the *order* things run in - a node's
    /// StepType is only used to label/bucket its log output, not to decide when it runs.
    /// </summary>
    public abstract class AUploadNodeV2
    {
        public abstract Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token);
    }

    /// <summary>
    /// Runs its children one at a time, in order. Stops (and fails) as soon as one child fails.
    /// </summary>
    public class SequenceNodeV2 : AUploadNodeV2
    {
        private readonly List<AUploadNodeV2> m_children;

        public SequenceNodeV2(List<AUploadNodeV2> children)
        {
            m_children = children;
        }

        public override async Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            foreach (AUploadNodeV2 child in m_children)
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                if (!await child.Run(task, report, token))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Runs all its children at the same time and waits for every one of them to finish.
    /// </summary>
    public class ParallelNodeV2 : AUploadNodeV2
    {
        private readonly List<AUploadNodeV2> m_children;

        public ParallelNodeV2(List<AUploadNodeV2> children)
        {
            m_children = children;
        }

        public override async Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            Task<bool>[] tasks = m_children.Select(c => c.Run(task, report, token)).ToArray();
            bool[] results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }
    }
}
