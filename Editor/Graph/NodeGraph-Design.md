# Node-Based Upload Authoring — Design

A visual, node-based authoring surface for the Build Uploader pipeline, built on Unity's **Graph Toolkit**
(`com.unity.graphtoolkit`). The graph is an authoring layer only: it compiles down to the existing
`UploadProfile` / `UploadConfig` model and runs through the unchanged `UploadTask` engine. Think JetBrains
TeamCity build chains or Unreal Blueprints — a **Start** node kicks off an **execution flow** through **groups**
that run their contained **operations** in parallel or sequence.

## Model: an execution-flow graph

The graph is a flow of execution, not a flow of data:

```
 [Start] ──exec──▶ [Sequential Group] ──exec──▶ [Parallel Group] ──▶ …
                     ├ Copy Folder block            ├ Copy Folder block
                     ├ Copy File block              └ Copy File block
                     └ Discord Message block
```

- **Start node** — the entry point; one `Start` exec output.
- **Group nodes** (`SequentialGroupNode`, `ParallelGroupNode`) — Graph Toolkit `ContextNode`s that *contain*
  operation `BlockNode`s. Each has an `In` and `Then` exec port so groups chain after Start and after each other.
- **Operation blocks** — self-contained `BlockNode`s that live inside a group:
  - `CopyFolderBlock` / `CopyFileBlock` — carry a full **Source Path** and **Destination Path** (plus zip /
    duplicate-handling). Each compiles to one complete `UploadConfig`.
  - `DiscordMessageBlock` — compiles to a post-action attached to the nearest preceding copy block in the group.

Connections are pure execution flow (`ExecFlow` port type). Operations no longer wire source→destination; each
operation carries the full paths it needs, per the "require a full path" requirement.

## Why these shapes (Graph Toolkit constraints)

Groups use `ContextNode` + `BlockNode`, the toolkit's only containment primitive. Blocks can't be placed on the
canvas directly — they're added inside a group via its "Add a Block" button — which is exactly the grouping
semantics wanted. `[UseWithContext(typeof(SequentialGroupNode), typeof(ParallelGroupNode))]` on each block makes
both group types accept it. The compiler reads a group's blocks via `ContextNode.blockNodes` (in order) and walks
the exec chain via the `IPort` connection API.

## Serialization

Two persistence systems stay separate:

- **Graph asset** (`.bugraph`) — Graph Toolkit serializes nodes, blocks, exec wires and option values. Authoring
  artifact.
- **Pipeline JSON** — `UploadProfile` / `UploadConfig` via each type's `Serialize()`/`Deserialize()` dictionary.
  Execution artifact.

The compiler bridges them through the **existing `Serialize()`/`Deserialize()` contracts** — each block builds the
exact dictionary the runtime type expects and feeds it to a fresh instance. No new setters, no reflection into
private fields, nothing renamed. The one subtlety: values the runtime reads as `long` (JSON numbers load as
`long`) must be boxed as `long` when built by hand — `LocalPathDestination`'s duplicate-handling and the Discord
IDs are the cases that matter.

## Compile + execution

`BuildUploaderGraphCompiler.Compile` walks Start → group → group and produces a `GraphPlan`: an ordered list of
`GroupPlan`s, each holding its `UploadConfig`s and its `GroupMode`.

`BuildUploaderGraphRunner.RunAsync` executes the plan:

- Groups run in flow order, each awaited before the next (the chain is sequential — "continue when its condition
  is done").
- **Parallel** group → all its configs run together in one `UploadTask`.
- **Sequential** group → one `UploadTask` per config, awaited in block order.

This delivers genuine parallel/sequential behaviour through the **unchanged** `UploadTask` engine — no engine
changes, batch mode / CLI untouched. `OnGraphChanged` runs the same compile and reports problems through
`GraphLogger` as node markers at author time.

## Files

```
Editor/Graph/
  VeugelJame.BuildUploader.Editor.Graph.asmdef   refs package + GraphToolkit asms; versionDefine gate
  BuildUploaderGraph.cs                           the [Graph] asset + OnGraphChanged validation
  GraphPipeline.cs                                ExecFlow, GroupMode, ICopyBlock/IActionBlock, GraphPlan/GroupPlan
  StartNode.cs                                    entry point
  Groups/AGroupNode.cs                            ContextNode base (exec In/Then)
  Groups/SequentialGroupNode.cs                   runs contained ops in order
  Groups/ParallelGroupNode.cs                     runs contained ops concurrently
  Nodes/ACopyBlock.cs                             shared copy logic (Source/Dest path → UploadConfig)
  Nodes/CopyFolderBlock.cs                        → FolderSource → LocalPathDestination
  Nodes/CopyFileBlock.cs                          → FileSource → LocalPathDestination
  Nodes/DiscordMessageBlock.cs                    → DiscordMessageChannelAction (post-action)
  BuildUploaderGraphCompiler.cs                   graph → GraphPlan
  BuildUploaderGraphRunner.cs                     plan → UploadTask(s) + menu item
  DiscordIconLoader.cs                            downloads/caches the Discord mark as a Texture2D
```

Everything is wrapped in `#if BUILD_UPLOADER_GRAPHTOOLKIT` (auto-defined by the asmdef when the package is
installed).

## API limitations (Graph Toolkit 0.4-exp.2)

Verified against the complete `Unity.GraphToolkit.Editor` namespace. The public surface is `Node` / `BlockNode` /
`ContextNode` / `Graph`, the port/option builders, and four attributes (`Graph`, `Subgraph`, `UseWithContext`,
`UseWithGraph`). There is **no public hook** for the following, so they are deferred until the package exposes them:

- **Custom node titles** — the node header title is the class name (the editor prettifies it, e.g.
  `CopyFolderBlock` → "Copy Folder Block"). No API to set an arbitrary display string.
- **Custom node icons** — no API to assign an icon to a node header. `DiscordIconLoader` still downloads and caches
  the Discord mark as a `Texture2D` (the requested half) so it's ready the moment an icon hook lands, or for use in
  any future custom inspector.
- **In-node browse ("...") buttons** — node options render with default field UI only; there is no custom in-node
  field/UI hook. Paths are plain string options for now.

## Roadmap

- Per-block exec ordering so a Discord (or other action) block can sit between two copies in a sequential group
  rather than attaching to the preceding copy.
- Modifier blocks on the copy operation (zip is currently a copy-block option).
- Reflection-generated blocks for every `[UploadSource]`/`[UploadDestination]`/`[UploadAction]` type.
- When Graph Toolkit exposes node-UI hooks: friendly titles, the Discord icon binding, browse buttons, and
  service-aware dropdowns (App/Server/Channel) replacing raw IDs.

## Caveats

Graph Toolkit is `0.4.x` experimental; signatures shift between point releases. Version-sensitive call sites:
`AddOption<T>(name).WithDisplayName().Build()`, `AddInputPort/AddOutputPort(...).Build()`, `IPort.GetNode()`
(the `INodeExtensions` extension), `ContextNode.blockNodes`, and `GraphDatabase.LoadGraph<T>` /
`PromptInProjectBrowserToCreateNewAsset<T>`. All are localized and easy to confirm against the installed version.
