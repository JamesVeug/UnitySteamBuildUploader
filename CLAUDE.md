# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Wireframe Build Uploader** — a Unity Editor-only UPM package (`com.veugeljame.builduploader`, namespace `Wireframe`, Unity 6000.3+, version 3.2.2). It lets developers configure upload pipelines (sources → modifiers → destinations → actions) that run when a build completes. There is no runtime code; everything lives under `Editor/`.

## Architecture: The Upload Pipeline

A pipeline (`UploadConfig`) contains four ordered lists, each serialized to JSON:

```
Sources       → what to upload (e.g. a build folder, a file)
Modifiers     → transforms on the source (e.g. zip, rename)
Destinations  → where to upload (e.g. Dropbox, Google Drive, FTP)
PostActions   → what to do after upload (e.g. send Slack/Discord/Google Chat message)
```

The pipeline runs through typed steps defined in `AUploadTask_Step.StepType`:
`Validation → PreUploadActions → PrepareSources → GetSources → CacheSources → ModifyCacheSources → PrepareDestinations → Upload → PostUploadActions → Cleanup`

Pipeline configs are persisted as JSON in `<ProjectRoot>/BuildUploader/UploadProfiles/*.json`. **Class names and serialization keys are written to JSON — never rename them.**

## Executing a Pipeline

### `UploadTask.cs` — the standalone entry point

`Editor/Core/UploadTask/UploadTask.cs` is the single starting point for running the pipeline. It wraps one or more `UploadConfig`s plus any `UploadActionData` and owns the entire execution lifecycle (steps, progress reporting, context wiring, the `UploadTaskReport`).

Typical use from code:

```csharp
// Build from a saved UploadProfile (preferred — picks up actions too)
UploadTask task = new UploadTask(profile);

// Or compose ad-hoc
UploadTask task = new UploadTask("My Upload", uploadConfigs, actions);
task.AddConfig(extraConfig);
task.AddAction(extraAction);

task.OnComplete += report => { /* inspect report.Successful, report logs */ };
task.Start();           // fire-and-forget async
// task.StartAndBlock();  // synchronous variant (blocks the calling thread)
// await task.StartAsync();
```

`UploadTask` is responsible for instantiating the step sequence (`UploadTaskStep_Validate` → … → `UploadTaskStep_Cleanup`), chaining the `UploadTaskStringFormatterContext` into every config and action, bumping the project upload number, and surfacing progress via `ProgressUtils`. **Do not reimplement this orchestration elsewhere — always go through `UploadTask`.**

### `BuildUploaderCommands` — the shared command layer

Every command lives in `Editor/Core/Utils/BuildUploaderCommands*.cs`, split across three files of one `internal static partial class`:

| File | Holds |
|------|-------|
| `BuildUploaderCommands.Args.cs` | The argument table: name/description `const`s, `CommandArg`, `Args`, `CommandRequest`, `BuildUsageString` |
| `BuildUploaderCommands.cs` | `Execute(CommandRequest)` plus every operation and its helpers |
| `BuildUploaderCommands.Results.cs` | The result classes returned to callers |

Two thin entry points sit on top, and **neither contains any logic** — each only turns its own argument syntax into a `CommandRequest`:

- `PipelineCommands.cs` — the `unity` CLI, against an already-open Editor. Gated on `BUILD_UPLOADER_PIPELINE`.
- `BatchModeUtil.cs` — `-executeMethod` in a headless Editor. Ungated.

**None of the three shared files may be gated or reference `Unity.Pipeline`** — batch mode has to reach the same commands when `com.unity.pipeline` is absent.

Everything must stay in the `VeugelJame.BuildUploader.Editor` assembly: `UploadTask.AllTasks` is `internal`, and registering a task there is what makes it visible to `--active_tasks`, the Upload Tasks window, and the concurrent-build guard in `ABuildSource`.

#### Adding an operation

Four edits, all in the shared layer, and both surfaces pick it up:

1. A `NameArg` and a `NameDescription` `const` in `BuildUploaderCommands.Args.cs`. The description must be a `const` because `[CliArg]` takes a compile-time constant — that is what stops the two surfaces' help text from drifting.
2. A `CommandArg` entry in `Args`, in the right group.
3. A handler in `Execute(...)` writing its own key into the result dictionary.
4. A `[CliArg]` parameter on `PipelineCommands.Command(...)` referencing the same two consts, plus the matching `request.Set` / `request.SetFlag`.

`BatchModeUtil` needs no edit — it parses against `Args`. The `usage` string is generated from `Args` too, so there is nothing to keep in sync.

Arguments are grouped and must stay in this order: **Listing → Inspection → Running → Mutations → Safety**. Each is independent, several may be combined in one invocation, and each writes its own key into the returned `Dictionary<string, object>`.

| Group | Arguments |
|-------|-----------|
| Listing | `profiles`, `active_tasks`, `source_types`, `modifier_types`, `destination_types`, `action_types`, `reports`, `cache_summary` |
| Inspection | `verify_profiles`, `verify_tasks`, `summarize_profiles`, `summarize_tasks`, `open_tasks` (+ `errors_only`) |
| Running | `start_tasks`, `dry_run_tasks`, `cancel_tasks` |
| Mutations | `clone_profiles` (+ `new_name`), `clone_tasks`, `delete_profiles`, `delete_tasks`, `clear_cache`, `export_wiki` |
| Safety | `confirm` |

Argument names are shared; only the prefix differs — `--profiles` through the pipeline CLI, `-profiles` in batch mode. Descriptions therefore refer to other arguments **without a prefix**, since one string serves both surfaces.

### `PipelineCommands.cs` — Unity CLI / agent execution

`Editor/Core/Utils/PipelineCommands.cs` exposes the commands to the `unity` CLI (and therefore to agents) through Unity's `com.unity.pipeline` package, which runs commands against an **already-open Editor** over a local HTTP server.

The whole file is wrapped in `#if BUILD_UPLOADER_PIPELINE`, a `versionDefines` entry in `veugeljame.builduploader.editor.asmdef` that is set whenever `com.unity.pipeline` (any version) is installed. The asmdef also references the `Unity.Pipeline` assembly. **Nothing in the package may depend on this file** — the package must compile and behave identically when `com.unity.pipeline` is absent.

```bash
unity command build_uploader --profiles true
```

```bash
unity command build_uploader --verify_profiles all --summarize_profiles all
```

Pipeline command names live in a single flat global namespace and **the first registration of a name wins**, so the package deliberately claims exactly one name — `build_uploader`. Do not add a second `[CliCommand]`; add an argument instead.

### `BatchModeUtil.cs` — headless / CI execution

`Editor/Core/Utils/BatchModeUtil.cs` runs the same commands in a headless Editor, which is the path CI takes. It walks `Environment.GetCommandLineArgs()` against `BuildUploaderCommands.Args`, ignoring anything it does not recognise because Unity passes plenty of its own arguments.

```
"PATH/TO/Unity.exe" -batchmode -quit -projectPath "PATH/TO/PROJECT" -logFile - \
    -executeMethod Wireframe.BatchModeUtil.Execute \
    -start_tasks "UPLOAD_PROFILE_GUID"
```

Three things are specific to this surface:

- **It blocks.** `CommandRequest.Blocking` is true unless `-async` is passed, so `StartProfile` uses `task.StartAndBlock()` and the process stays alive until the upload finishes. Blocking deliberately reuses `StartAndBlock` rather than a wait loop of its own — `StartAsync` awaits `Task.Yield()` throughout and those continuations post to the main-thread sync context, so a home-grown wait risks a deadlock that only shows up in a real batch run.
- **It sets an exit code.** A failed task, an unresolvable GUID, a refused destructive operation, or no operation at all logs an error and calls `EditorApplication.Exit(1)`. The exit is guarded on `Application.isBatchMode` so calling `Execute` from a live Editor never closes it. On success nothing is forced and `-quit` exits normally.
- **It prints the result.** The `Dictionary<string, object>` is serialized with the package's own `JSON` class and fenced between `[BuildUploader] BEGIN_RESULT` and `[BuildUploader] END_RESULT` so a CI job can lift it out of the Unity log.

Flags may be bare (`-profiles`) or take an explicit value (`-profiles true`), the latter because callers carry the habit over from the pipeline CLI.

**`-uploadProfile <GUID>` is deprecated.** It still works — it logs a warning and is appended to `start_tasks`, and repeated pairs still accumulate — but new callers should use `-start_tasks`, which also accepts a profile name, takes several values, and understands `all`.

Operations needing live task state (`-active_tasks`, `-cancel_tasks`, `-clone_tasks`) only see tasks started by the same invocation, since a batch process starts with an empty `UploadTask.AllTasks`. Everything backed by a saved report reads from disk and works normally.

#### Argument conventions

- **Multi-value args are strings, not arrays.** They arrive as `"a,b"`, `"a b"`, or a JSON array when the caller quotes one; `SplitArgument` normalises all three and returns an empty list rather than null. Always route a new multi-value arg through it.
- **Profiles are addressed by GUID *or* name.** `ResolveProfileMeta` / `ResolveProfile` accept either and throw `ArgumentException` when nothing matches. Tasks are addressed by GUID only.
- **`all` expands, and what it expands to depends on the operation.** `ExpandProfiles` turns it into every `UploadProfileMeta` GUID; `ExpandTasks` takes a `TaskScope`:
  - `TaskScope.Known` — in-memory tasks plus every saved report (verify, summarize, open, delete).
  - `TaskScope.InMemory` — only tasks from this editor session, the only ones that still hold their config (`--clone_tasks`).
  - `TaskScope.Running` — in-memory tasks that have not completed (`--cancel_tasks`).
  `IsAllArgument` also accepts `true` and `*`, because callers reach for `true` out of habit on flag-shaped arguments.
- **Live tasks win over saved reports.** `ResolveReport` checks `UploadTask.AllTasks` first (it has progress a file does not) and reports which it used via the `live` field.

#### Running tasks

`start_tasks` and `dry_run_tasks` both go through `StartProfile`, which registers the task in `UploadTask.AllTasks` and then either blocks or does not, depending on `CommandRequest.Blocking`:

- **Through the pipeline CLI, `Blocking` is always false** and `task.Start()` returns immediately. It must — the pipeline server dispatches commands on the main thread the task itself needs. Callers poll `--active_tasks` and then read `--summarize_tasks` / `--open_tasks`.
- **In batch mode it defaults to true** (`-async` turns it off) and uses `task.StartAndBlock()`, so the process outlives the upload and can report an exit code.

`dry_run_tasks` clones the profile in memory (`CloneProfileInMemory`, a JSON round-trip through `UploadProfileSavedData`) and swaps every destination for `NoUploadDestination`, so sources and modifiers run but nothing is uploaded and the profile on disk is untouched.

#### Destructive operations

`delete_profiles`, `delete_tasks`, and `clear_cache` refuse to run without `confirm`. `RequireConfirmation` will offer an `EditorUtility.DisplayDialog` when a human is actually at the Editor, but returns false in batch mode or when `InternalEditorUtility.isHumanControllingUs` is false — a modal dialog would block the main thread the server runs on, and there is nobody to answer it headless. It takes the caller's argument prefix so the refusal message tells the user the right spelling. **Any new destructive argument must go through `RequireConfirmation` and be listed in `ConfirmDescription`.** `clear_cache` deliberately preserves the saved reports living inside the cache folder; `delete_tasks` is the way to remove those.

#### Result classes

Every operation returns a class from `BuildUploaderCommands.Results.cs`. **Field names are the JSON keys the caller sees**, so they are lower-camel-cased to match the rest of the pipeline commands rather than following normal C# casing — keep new fields in that style, and treat renames as a breaking change for anyone scripting against the output. Collection fields are initialised inline so a result never serialises a null list.

Failures are thrown as `ArgumentException` / `InvalidOperationException`. Through the pipeline CLI they surface as a CLI error; `BatchModeUtil` catches them, logs them, and exits 1 rather than letting an unhandled exception leave the process reporting success. The exception is per-item niceties which are allowed to fail (e.g. `Summary()` on a profile read straight off disk, or reading drive free space) — those are caught and reported inline as `<unavailable: ...>` rather than failing the whole command.

## Build Configs and Build Profiles

The package supports two interchangeable ways to describe "how to build the player" for a `BuildConfigSource` / `BuildProfileSource`. Both implement the same `IBuildConfig` interface (`GetBuildName`, `GetGUID`, `GetTargetPlatform`, `GetTarget`, `GetTargetPlatformSubTarget`, `GetTargetArchitecture`, `GetSceneGUIDs`, `GetProductName`, `GetScriptingBackend`, `GetBuildOptions()`, `GetProductExtension()`, `GetFormattedProductName(ctx)`, `ApplySettings(switchPlatform, ctx, stepResult)`), so everything downstream — `ABuildSource<T>`, validation, the upload pipeline — is agnostic to which one the user picked.

### `BuildConfig` — the package's own config (all Unity versions)

`Editor/Core/UploadTask/BuildConfig.cs`. A serializable POCO authored in **Project Settings → Build Uploader → Build Configs** (`ProjectSettings_BuildConfigs`). Persisted as JSON in `<ProjectRoot>/BuildUploader/BuildConfigs.json`; load via `BuildConfigsUIUtils.GetBuildConfigs()`, `BuildConfig.FromGUID(guid)`, or `BuildConfig.FromBuildName(name)`. Two defaults — "Debugging Build" and "Release Build" — are written out the first time a project opens the settings page.

Stored fields cover scenes (`SceneGUIDs`), product name, `ExtraScriptingDefines`, dev-build flags (`IsDevelopmentBuild`, `BuildScriptsOnly`, `AllowDebugging`, `ConnectProfiler`, `EnableDeepProfilingSupport`), platform (`SwitchTargetPlatform`, `TargetPlatform`, `TargetPlatformSubTarget`, `Target`, `TargetArchitecture`), and player settings (`StackTraceLogTypes`, `StrippingLevel`, `ScriptingBackend`, `CompressionMethod`). `ApplySettings()` writes every one of these to `PlayerSettings` / `EditorUserBuildSettings` / `EditorBuildSettings.scenes` and optionally switches platform via `BuildUtils.TrySwitchPlatform`.

`BuildConfig` implements `DropdownElement` (`Id` is for popup order and is rewritten when entries are reordered; `GUID` is the stable 6-char identifier serialized into `BuildConfigSource`). **Never reuse a deleted GUID** — sources reference configs by `GUID`. `Serialize()` / `Deserialize()` keys are written to JSON, so do not rename them.

`BuildConfigSource` additionally serializes an optional platform override (`m_OverrideSwitchTargetPlatform` + `m_Target` / `m_TargetPlatform` / `m_TargetPlatformSubTarget` / `m_TargetArchitecture`) that wins over the config's own `SwitchTargetPlatform` when set — `GetBuildConfigToApply()` deep-clones the `BuildConfig` so the saved asset is never mutated.

### `BuildProfile` — Unity's native asset (Unity 6000.0+ only)

Wrapped read-only by `BuildProfileWrapper : IBuildConfig, DropdownElement` (`BuildProfileWrapper.cs`, gated on `UNITY_6000_0_OR_NEWER`). The package does not author, edit, or persist `BuildProfile` assets — it only references them by `GUID`. Discovery goes through `BuildUtils.GetAllCustomBuildProfiles()`, which version-gates between `BuildProfile.GetAllBuildProfiles()` (6000.5+), `BuildProfileModuleUtil.GetAllBuildProfiles` (6000.3+), and the legacy `BuildProfileDataSource.FindAllBuildProfiles` (earlier 6000.x). The wrapper reaches into `BuildProfile` via reflection for `playerSettings`, `platformBuildProfile`, `buildTarget`, `subtarget`, and dev-build flags — those internal APIs have moved between Unity 6000.x point releases, so additions there must go through the existing reflection helpers in `BuildProfileWrapper`.

`BuildProfileUIUtils` caches wrappers, refreshes on `[DidReloadScripts]`, and subscribes to `BuildProfile.AddOnBuildProfileCreated` via reflection so the dropdown stays in sync when the user creates a new profile.

`BuildProfileSource.ApplyBuildConfig` is a no-op — Unity owns settings application — and `MakeBuild` calls `BuildPipeline.BuildPlayer(options)` after `BuildProfile.SetActiveBuildProfile(...)`. Anything that needs override behaviour (override platform, override product name, override scenes) belongs in `BuildConfigSource`, not the profile path.

### `ABuildSource<T>` — the shared base

`Editor/Core/UploadTask/DownloadSources/ABuildSource.cs` is the common base for both sources. It owns the build loop, the global single-build lock (`BuildUtils.WaitForTurnToBuild` / `ReleaseBuildLock`, backed by `m_lock` semaphore + `ChangeRunningBuilds` counter), pre/post settings caching (so editor state is restored only after the *last* concurrent build cleans up), `BuildOptions.DetailedBuildReport` / `BuildOptions.CleanBuildCache` plumbing, and the `report.steps` → `stepResult` log forwarding. Subclasses only have to implement `GetBuildConfigToApply()`, `CompareBuildConfig(IBuildSource)`, `SerializeBuildConfig` / `DeserializeBuildConfig`, and optionally override `ApplyBuildConfig` and `MakeBuild`.

`TryGetErrors` (run during the `Validation` step) blocks a build when: the selected config is null, scenes are missing or unresolvable, `BuildUtils.GetBuildPlatform()` returns null, the platform is not `installed`, the platform is not `supported`, or another active `UploadTask` is already mid-build with the same `IBuildConfig` (matched via `CompareBuildConfig`). Everything that can be answered from config alone goes here, per the destination-contract rule.

## Supported Platforms

There is no hardcoded platform list. `BuildUtils.ValidPlatforms` is built lazily via reflection over Unity's internal `BuildPlatforms.instance.buildPlatforms`, so **every `BuildTargetGroup` Unity exposes plus has a module installed is available** — `Standalone`, `Android`, `iOS`, `WebGL`, `WSA`, consoles, etc. The package treats them uniformly through `BuildUtils.GetBuildPlatform(group, target, subTarget)` and routes per-platform quirks through a small set of utilities rather than `switch` ladders.

Three behaviours need attention when adding code that touches platforms:

**Standalone is split per OS.** `BuildUtils.GetValidPlatforms()` clones the single Unity Standalone entry into three rows — `StandaloneWindows64`, `StandaloneOSX`, `StandaloneLinux64` — each with its own `installed` / `supported` flags computed against `IsTargetGroupInstalled` and `IsTargetGroupSupported`. `BuildUtils.GetPlatformExtension` maps these to `.exe`, `.app`, `.x86_64` respectively.

**Architecture is Standalone-only.** `BuildUtils.Architecture` is `x64 | ARM64 | x64ARM64 | x86`, with `x64ARM64` macOS-only and `x86` Windows-only. `DrawArchitecturePopup` short-circuits for `targetPlatform != BuildTargetGroup.Standalone` and returns the input unchanged. Android architecture is handled inside Unity's own player settings and isn't surfaced through this enum.

**Android has its own output extension.** `GetPlatformExtension` returns `.aab` when the bundle flag is set, otherwise `.apk`. `BuildProfileWrapper.GetProductExtension` reads `buildAppBundle` off the platform-settings object via reflection; `BuildConfig.GetProductExtension` currently hardcodes `androidBundle: false` (see the inline comment "Android not supported atm" — adding bundle support there means propagating the flag through `BuildConfig` + its GUI).

**Module-installed checks.** `IsTargetGroupInstalled` and `IsTargetGroupSupported` are the canonical "can we actually build this?" probes; on Unity 6000.0+, `IsTargetGroupSupported` calls `BuildPipeline.IsBuildPlatformSupported` via reflection because the public overload was removed. `TrySwitchPlatform` further enforces that Standalone requires a non-zero `TargetPlatformSubTarget` (Unity 2021.1+) or non-`-1` (Unity 6000.0+).

When introducing new platform-specific behaviour, route it through `BuildUtils` rather than scattering `BuildTargetGroup`/`BuildTarget` switches across services or sources.

## Reflection-Based Registration

Types are discovered automatically via `InternalUtils.FetchAllTypes()`, which scans all assemblies. Registration uses attributes:

- `[UploadAction("Display Name")]` → appears in the PostActions picker
- `[UploadDestination("Display Name")]` → appears in the Destinations picker
- `[UploadSource("Display Name")]` → appears in the Sources picker
- `[UploadModifier("Display Name")]` → appears in the Modifiers picker
- `AService` subclasses → discovered automatically via `InternalUtils.AllServices()`

**Never manually register types.** Adding the attribute and subclassing the base is sufficient.

## Partial Class Conventions

Every non-trivial class is split across three files:

| Suffix | Purpose |
|--------|---------|
| `.cs` | Core logic: fields, Execute/Upload, TryGetErrors, Serialize, Deserialize |
| `.GUI.cs` | Editor UI: `OnGUICollapsed` and `OnGUIExpanded` |
| `.StringContextModifier.cs` | Registers format-string keys the action produces as output |

## Context / FormatString System

The `Context` class resolves `{key}` tokens in user-authored strings at runtime.

- `context.AddCommand(defaultKey, getterFunc, tooltip)` — registers a named slot; returns a `Command` object stored as a field
- `m_context.FormatString(str)` — **must be called on every user string before use**
- Contexts chain: each action inherits the pipeline's parent context

The `.StringContextModifier.cs` partial is only needed when an action *produces* a value (e.g. a message ID) that a later action needs to consume via `{key}`.

## Data Storage Tiers

| Tier | Where | Used for |
|------|-------|---------|
| `EditorPrefs` | Per-machine registry | Secrets that are shared across all projects on this machine: tokens, API keys, passwords |
| `ProjectEditorPrefs` | Per-machine registry, namespaced by a per-project UUID stored at `../BuildUploader/ProjectID.txt` | Secrets and per-project flags that must NOT leak between projects on the same machine (project-scoped tokens, enabled toggles, per-project API keys) |
| JSON config files | `../BuildUploader/<ServiceName>Config.json` | Non-secret config: server names, channel IDs, space names |

`ProjectEditorPrefs` is a thin wrapper over `UnityEditor.EditorPrefs` that prefixes every key with the project UUID. It exposes the same shape as `EditorPrefs` (`SetBool/GetBool`, `SetInt/GetInt`, `SetString/GetString`, `SetFloat/GetFloat`, `DeleteKey`, `HasKey`) plus `MigrateFromEditorPrefs(key, PrefType)` for moving legacy keys over. Use it for any secret or flag that should be scoped to *this project on this machine* rather than to all projects globally.

**Tokens and API keys must never be stored in JSON config or serialized ScriptableObjects.** Choose `EditorPrefs` only when the credential is genuinely shared across every project on the machine; otherwise prefer `ProjectEditorPrefs`.

## HTTP Calls

Always use `RequestWrapper`. Never use `HttpClient` or `UnityWebRequest` directly.

```csharp
using (RequestWrapper www = RequestWrapper.Post(url))   // also .Get, .Delete, .Patch
{
    www.SetJSONData(body);                              // Dictionary<string, object>
    www.SetRequestHeader("Authorization", $"Bearer {token}");
    RequestResult response = await www.SendAsync(result, true);
    if (!response.IsSuccessful) { /* handle */ }
    // response.Data → string JSON; response.Bytes → byte[]
}
```

**Never put secrets in the URL.** `RequestWrapper` logs the full request line (`www.method + " " + www.url`) to the `UploadTaskReport` via `result.AddLog(...)` on both send and success. Those reports are surfaced in the Upload Tasks window and are routinely copied into bug reports, chats, and issues — so anything in the URL is effectively public. If a secret has to travel in the URL (e.g. a Slack/Discord webhook, a signed-URL token, an API key query param), mask the sensitive segment with `xxx-xxx-xxx` in the logged URL before it reaches `AddLog`. Prefer carrying credentials in a header (`SetRequestHeader`) over the URL whenever the remote API allows it, since headers are not logged.

## JSON Serialization

Always use the package's own `JSON` class (`Editor/Core/Utils/JSON/`) to serialize and deserialize JSON. Never use `JsonUtility`, `Newtonsoft.Json`, or any external JSON library — the package is dependency-free and `JsonUtility` cannot handle the shapes used here (e.g. `Dictionary<string, object>`, polymorphic config lists).

```csharp
string json = JSON.SerializeObject(data);              // T or object
string json = JSON.SerializeObject<MyType>(data);

MyType data = JSON.DeserializeObject<MyType>(json);    // generic
object data = JSON.DeserializeObject(json, type);      // runtime Type
```

This is the canonical path for `Serialize()` / `Deserialize()` on actions, sources, modifiers, destinations, and for reading/writing the JSON config files under `<ProjectRoot>/BuildUploader/`. HTTP request bodies built as `Dictionary<string, object>` and passed to `RequestWrapper.SetJSONData(...)` go through this same serializer.

## Service Pattern

Each service (Slack, Discord, Google Chat, etc.) has:

```
Services/<Name>/
  <Name>Service.cs                        AService subclass (ServiceName, SearchKeywords, IsReadyToStartBuild, IsProjectSettingsSetup)
  <Name>Service.Preferences.cs           Renders Edit → Preferences → Build Uploader → Services → <Name>
  <Name>Service.ProjectSettings.cs       Renders Project Settings → Build Uploader → Services → <Name>
  <Name>Service_Preferences_Provider.cs  SettingsProvider wiring (SettingsScope.User)
  <Name>Service_ProjectSettings_Provider.cs  SettingsProvider wiring (SettingsScope.Project)
  Data/
    <Name>Config.cs                       Serializable config root (list of servers/spaces/channels)
    <Name>UIUtils.cs                      GetConfig(), Save(), popup helpers (AppPopup, ServerPopup, ChannelPopup)
    <Name><Entity>.cs                     Each config entity: implements DropdownElement (int Id, string DisplayName)
  API/
    <Name>.cs                             Static partial class — pure HTTP calls only, no UI
  Actions/
    <Name><Verb>ChannelAction.cs          AUploadAction subclass
    <Name><Verb>ChannelAction.GUI.cs      Editor UI partial
    <Name><Verb>ChannelAction.StringContextModifier.cs  Context key registration (if action produces output)
  ReorderableListOf<Name><Entities>.cs   InternalReorderableList<T> subclasses for list UI
```

### `DropdownElement` (every config entity implements this)

```csharp
public interface DropdownElement
{
    int Id { get; }
    string DisplayName { get; }
}
```

Every entity that a user can pick from a popup — a Slack channel, a Discord server, an itch.io game, a Steam depot, an Apple beta group — implements `DropdownElement`. The contract is intentionally tiny:

- `Id` — a stable integer identifier used for serialization and equality. It must remain stable across editor sessions; new entries get a fresh `Id` (typically `list.Count + 1` at creation time inside the matching `ReorderableListOf<…>.CreateItem`). **Never reuse a deleted `Id`.**
- `DisplayName` — the human-readable label shown in popups, dropdowns, and reorderable list rows. Free to change at any time; UI only.

Serialized references to entities are stored by `Id`, never by `DisplayName`. This is what lets `CustomDropdown<T>` and `CustomMultiDropdown<TParent,TChild>` resolve a saved selection back to the current entity even if the user has since renamed it.

Popup helpers use `CustomDropdown<T>` (single) or `CustomMultiDropdown<TParent,TChild>` (hierarchical). `DrawPopup(ref T value, Context ctx, GUILayout.Width(n))` returns `isDirty`.

### `InternalReorderableList<T>` (every service uses this)

Every service surfaces its config entities (channels, servers, depots, branches, attachments, …) through a subclass of `InternalReorderableList<T>` (`Editor/Core/Utils/InternalReorderableList.cs`). This is a thin wrapper around `UnityEditorInternal.ReorderableList` that standardises the foldout header, add/remove/reorder callbacks, right-click context menu (Clear, Sort Ascending/Descending), and the `dirty` flag that drives saving.

Subclass contract:

```csharp
public class ReorderableListOfSlackChannels : InternalReorderableList<SlackConfig.SlackChannel>
{
    // Required: paint one row
    protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused) { … }

    // Required: produce a new entity when the user clicks +
    // The index passed in is intended as the seed for DropdownElement.Id
    protected override SlackConfig.SlackChannel CreateItem(int index)
        => new SlackConfig.SlackChannel(index, "BotTestChannel", "");

    // Optional: override sort order for the context-menu Sort items
    protected override int CompareTo(SlackConfig.SlackChannel a, SlackConfig.SlackChannel b)
        => string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
}
```

Usage from a Preferences / ProjectSettings GUI:

```csharp
m_channelsList = new ReorderableListOfSlackChannels();
m_channelsList.Initialize(config.Channels, "Channels", foldoutStartsOpen: true);
…
if (m_channelsList.OnGUI())      // returns true when the list is dirty
{
    SlackUIUtils.Save();          // persist the JSON config
}
```

Conventions:

- One `ReorderableListOf<Entity>` per entity type. If the same entity is rendered differently in Preferences vs ProjectSettings (e.g. masking secret fields), make two subclasses — see `ReorderableListOfSlackAppsPreferences` vs `ReorderableListOfSlackAppsProjectSettings`.
- Set `dirty = true` from inside `DrawItem` whenever a field changes; the framework forwards that back to the caller via `OnGUI()`'s return value.
- Use `EditorGUILayout` only inside the foldout-header `HorizontalScope`; row drawing is rect-based via `GUI.*` because `ReorderableList` provides explicit `Rect`s.
- Never call `ReorderableList` directly — always go through `InternalReorderableList<T>` so the header, foldout, and context menu stay consistent across services.

## Key Base Classes

- **`AUploadAction`** — `Prepare()`, `Execute()`, `CleanUp()`, `TryGetErrors(List<string>)`, `TryGetWarnings(List<string>)`, `Serialize() → Dictionary<string,object>`, `Deserialize(Dictionary<string,object>)`
- **`AService`** — `ServiceName`, `SearchKeywords`, `IsReadyToStartBuild(out string)`, `IsProjectSettingsSetup()`, `PreferencesGUI()`, `ProjectSettingsGUI()`
- **`AUploadSource`** — `Summary()`, `Prepare()`, `GetSource(doNotCache, …)`, `SourceFilePath()`, `CleanUp()`, `TryGetErrors`/`TryGetWarnings`, `Serialize`/`Deserialize`, `PrepareContextForCaching()`. Subclasses are tagged with `[UploadSource("Display Name")]`.
- **`AUploadModifer`** — note the historical single-`i` spelling; class name is **written to JSON**, do not rename. Surface: `ModifyBuildAtPath(cachedFolderPath, uploadConfig, configIndex, stepResult, ctx)`, optional `IgnoreFileDuringCacheSource(...)` (lets a modifier skip files at the cache-copy stage instead of after), three `TryGetErrors`/`TryGetWarnings` overloads (against the `UploadConfig`, a specific `AUploadSource`, and a specific `AUploadDestination`), `Serialize`/`Deserialize`. Subclasses are tagged with `[UploadModifier("Display Name")]`.
- **`AUploadDestination`** — `Summary`, `Prepare()`, `Upload()`, `PostUpload()`, `CleanUp()`, `Serialize()`, `Deserialize()`

## Destination Upload Contract

`AUploadDestination.Upload()` (and `Prepare`, `PostUpload`, `CleanUp`) **must be written as if every precondition is already satisfied**: credentials are present and valid, the cached source exists, the target server/bucket/branch is reachable, quotas are not exceeded, etc. The upload path is the hot path and should not be cluttered with defensive validation that re-asks questions the user already answered.

All such validation belongs in `TryGetErrors(List<string> errors)` (hard failures that must block the task) and `TryGetWarnings(List<string> warnings, Context ctx)` (soft issues worth surfacing). These run during the `Validation` step of `UploadTask`, *before* any source is fetched or any byte is uploaded, so the user sees configuration problems up front instead of mid-upload.

Rule of thumb: if a condition can be checked from the serialized config alone (missing token, empty channel ID, unresolvable `{key}`, invalid path), it goes in `TryGetErrors`. If the condition can only be discovered by actually contacting the remote service, it stays in `Upload` and is reported via `stepResult` — but everything that can be caught earlier *should* be.

## Adding a New Service (Quick Reference)

1. Create the folder structure above under `Editor/Services/<Name>/`
2. Subclass `AService` — no registration needed
3. Create `<Name>Config` with entities that implement `DropdownElement`
4. Create `<Name>UIUtils` with `GetConfig()`, `Save()`, and popup helpers
5. Create the API class using `RequestWrapper` only
6. Create action classes with `[UploadAction("...")]` — no registration needed
7. Wire up two `SettingsProvider` classes for Preferences and ProjectSettings
8. Annotate user-facing types and fields with `[Wiki(...)]` so they appear in the exported documentation

Slack is the canonical reference implementation for all patterns.

## Documentation: `Editor/Core/Wiki`

The folder `Editor/Core/Wiki/` is the source of truth for the public-facing wiki at https://github.com/JamesVeug/UnityBuildUploader/wiki. It contains two files:

- `WikiAttribute.cs` — defines `[Wiki(name, subpath?, text, order?)]` and `[WikiEnum(...)]`. Apply to any class, field, or enum value that should be documented. `SubPath` must be one of `"sources"`, `"modifiers"`, `"destinations"`, `"actions"`. `TryGetWikiLink(type, out url)` resolves an in-editor "?" button to the right wiki page.
- `Wiki.cs` — gated behind `BUILD_UPLOADER_WIKI`. Adds two menu items:
  - **Window → Build Uploader → Export Wiki Data** walks every `[Wiki]`-decorated type in the assembly, groups them by `SubPath`, and rewrites `<ProjectRoot>/Wiki/Sources.md`, `Modifiers.md`, `Destinations.md`, `Actions.md`, `StringFormatter.md`, and `CLI.md`. Existing markdown above the `## Sources` / `## Modifiers` / `## Destinations` / `## Actions` / `## Commands` header is preserved; everything from that header onward is regenerated.
  - **Window → Build Uploader → Open Wiki Export Folder** reveals `<ProjectRoot>/Wiki/` in the OS file browser.

When adding a new source, modifier, destination, or action, decorate the type and any user-relevant fields with `[Wiki(...)]` so the export picks them up. The `StringFormatter.md` page is auto-generated from `Context.FormatToCommand`, so any `AddCommand(...)` call with a tooltip flows through automatically.

`CLI.md` is generated by `WriteCLICommands()` from **`BuildUploaderCommands.Args`**, not from the `[CliArg]` attributes. That matters: the attributes only exist under `BUILD_UPLOADER_PIPELINE`, so reflecting over them would put the generator at the intersection of two independent defines and produce a page missing the batch-mode surface whenever `com.unity.pipeline` is absent. Reading the shared table instead means the page is correct either way. Arguments are written in **declaration order**, one table per group, and both surfaces share one table because they share one set of names. **To document an argument, edit its `…Description` const** — there is nothing to hand-write.

The hand-authored preamble above `## Commands` covers the two entry points, the GUID-or-name/`all` conventions, and confirmation; `Wiki.cs` seeds it via `DefaultCLIPreamble()` only when the file does not exist yet.

`TryGetHandAuthoredPreamble(path, header, defaultPreamble, out preamble)` is the shared "keep everything above the generated header" helper used by both the `[Wiki]`-attribute pages and `CLI.md`. It creates the file seeded with the default when missing, and returns false — leaving the file untouched rather than emptying it — when the header cannot be found. New generated pages should go through it rather than re-implementing the preserve logic.

**Workflow:** edit code → add/adjust `[Wiki]` attributes → run **Export Wiki Data** → review the markdown diff under `<ProjectRoot>/Wiki/` → commit the regenerated files to the wiki repo. Do not hand-edit the auto-generated sections; edit the attribute text instead.

### Wiki repo layout (`<ProjectRoot>/Wiki/`)

`<ProjectRoot>/Wiki/` is a separate git repo (the GitHub wiki) checked out alongside the package — at `C:\GitProjects\SteamBuildUploader\Wiki` on this machine. It contains two kinds of pages:

> **Access note:** the wiki repo is a *separate* folder from this package and is often not mounted in the working session. Before writing or editing any wiki page (`Home.md`, `How-to-*.md`, the auto-generated `*.md`, etc.), confirm you actually have write access to `C:\GitProjects\SteamBuildUploader\Wiki`. If you do not, **ask the user to grant access to the wiki project folder** (e.g. via the folder-access request flow) rather than writing wiki files into the package, the outputs scratchpad, or anywhere else. Only fall back to delivering the file elsewhere if the user declines to grant access.

**Auto-generated (do NOT hand-edit below the `## Sources` / `## Modifiers` / `## Destinations` / `## Actions` header):**

- `Sources.md`, `Modifiers.md`, `Destinations.md`, `Actions.md` — reference pages built from `[Wiki(...)]` attributes by **Export Wiki Data**. The preamble above the header is hand-authored and preserved; everything after the header is regenerated each run.
- `StringFormatter.md` — fully regenerated from `Context.AddCommand(...)` tooltips. To add or change a documented `{key}`, edit the tooltip on the `AddCommand` call, not the markdown.
- `CLI.md` — the command reference for both the `unity command build_uploader` and the `-executeMethod Wireframe.BatchModeUtil.Execute` surfaces, regenerated from `BuildUploaderCommands.Args`. Preamble above `## Commands` is hand-authored and preserved; the argument tables are not. Regenerated whether or not `com.unity.pipeline` is installed.

**Hand-authored (safe to edit directly):**

- `Home.md` — wiki landing page; narrates the pipeline steps with semantics not encoded in code (e.g. "if any Upload Config fails the whole task cancels — exception: the Upload step does not cancel siblings").
- `How-to-Install.md` — UPM install steps (Asset Store and `https://github.com/JamesVeug/UnityBuildUploader.git`, plus `com.unity.sharp-zip-lib` for Unity 2020 and below).
- `How-to-Setup.md` — end-to-end first-run guide: Preferences → ProjectSettings → Upload Config → Upload. Canonical reference for what each service's setup flow looks like from the user side.
- `How-to-Create-a-new-Source.md`, `How-to-Create-a-new-Modifier.md`, `How-to-Create-a-new-Destination.md` — full copy-paste reference implementations (`LastBuildSource`, `CompressModifier`, `LocalPathDestination`). **When scaffolding a new source / modifier / destination, mirror the structure shown in the matching page** — it's the contract these pages are documenting.
- `Starting-a-BuildTask-without-UI.md` — three headless entry points (the `-executeMethod Wireframe.BatchModeUtil.Execute -start_tasks <GUID>` CLI form, the **Window → Build Uploader → Quick Upload → Generate Menu Items** generator which drops a file under `Assets/ThirdParty`, and the raw `new UploadTask(profile)` / `new UploadConfig(...)` C# API). Load profiles with `UploadProfile.FromGUID(guid)`, `UploadProfile.FromProfileName("Default")`, or `UploadProfile.FromPath(path)` — there is no `UploadProfile.From`. Set the build description via `UploadTask.SetBuildDescription(...)`. **This page still documents the deprecated `-uploadProfile <GUID>` form and needs updating** for `-start_tasks` and the rest of the batch surface.
- `Roadmap.md` — pointer to the GitHub issues/milestones; nothing automated.

### Pipeline runtime facts not encoded in code

- A failure in any step **before** Upload cancels every sibling config; a failure in the Upload step is reported but does not cancel the other configs' uploads.
- Cache root defaults to `%userprofile%/appdata/locallow/<companyName>/<productName>/BuildUploader/CachedBuilds`, configurable via Edit → Preferences → Build Uploader → General.
- `NowhereDestination` plus disabling cache cleanup in Preferences is the supported way to dry-run sources and modifiers without touching any remote service.
- Reports for past runs live in **Window → Build Uploader → Open Upload Tasks Window** — there is no completion dialog when running outside the GUI.
