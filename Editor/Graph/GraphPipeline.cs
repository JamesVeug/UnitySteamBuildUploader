#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using System.Collections.Generic;

namespace Wireframe
{
    /// <summary>
    /// Execution-flow token passed between the Start node and group nodes. Carries no data; it exists so Graph
    /// Toolkit only connects exec outputs to exec inputs (Start.Start -> Group.In, Group.Then -> next Group.In).
    /// </summary>
    [Serializable]
    public sealed class ExecFlow { }

    /// <summary>How a group runs the operations it contains.</summary>
    public enum GroupMode
    {
        /// <summary>Operations run one after another; the group completes when the last one finishes.</summary>
        Sequential,

        /// <summary>Operations run concurrently; the group completes when all of them finish.</summary>
        Parallel,
    }

    /// <summary>
    /// A self-contained operation block that compiles to a complete <see cref="UploadConfig"/> (its own source +
    /// destination). One block ≈ one config.
    /// </summary>
    public interface ICopyBlock
    {
        UploadConfig CompileConfig(GraphCompileLog log);
    }

    /// <summary>
    /// A block that compiles to a post-action (<see cref="UploadConfig.UploadActionData"/>). It is attached to the
    /// config produced by the nearest preceding copy block in the same group.
    /// </summary>
    public interface IActionBlock
    {
        UploadConfig.UploadActionData CompileAction(GraphCompileLog log);
    }

    /// <summary>Collects validation/compile feedback for editor validation and headless runs.</summary>
    public sealed class GraphCompileLog
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();

        public bool HasErrors => Errors.Count > 0;

        public void Error(string message) => Errors.Add(message);
        public void Warning(string message) => Warnings.Add(message);
    }

    /// <summary>One group's compiled output: the configs to run, and how to run them.</summary>
    public sealed class GroupPlan
    {
        public string Name;
        public GroupMode Mode;
        public readonly List<UploadConfig> Configs = new List<UploadConfig>();
    }

    /// <summary>The whole graph compiled into an ordered list of groups (Start → group → group …).</summary>
    public sealed class GraphPlan
    {
        public readonly List<GroupPlan> Groups = new List<GroupPlan>();
    }
}
#endif
