# UploadTask integration tests

Editor tests are separated into Common, Steam, Discord, Itchio and Builds assemblies. Add `com.veugeljame.builduploader` to the host project's `testables` array to discover them.

Every case executes `UploadTask.StartAsync` and requires a completed, successful task report:

- Steam: `FolderSource` → `SteamUploadDestination` with preview enabled and branch `none`. SteamCMD uses an existing authorized credential cache. No live build is published.
- Discord: `FolderSource` → `LocalPathDestination`, followed by one `DiscordMessageChannelAction` on task completion. Dry run serializes the formatted message and embed without sending HTTP.
- Itchio: an HTML fixture through `FolderSource` → `ItchioDestination`, channel `html5`, using native Butler `push --dry-run`. This checks the web upload pipeline; the fixture is not a compiled WebGL player.
- Builds: one player for the active Windows x64, macOS or Linux x64 target through a build source → `LocalPathDestination`. Unity 6 uses `BuildProfileSource`; Unity 2019/2022 use `BuildConfigSource` initialized from Build Settings.

## Configuration

Steam environment variables:

```
BUILDUPLOADER_TEST_STEAM_SDK=/absolute/path/to/steamworks-sdk
BUILDUPLOADER_TEST_STEAM_USER=authorized-build-account
BUILDUPLOADER_TEST_STEAM_APP_ID=app-id
BUILDUPLOADER_TEST_STEAM_DEPOT_ID=depot-id
```

The SDK directory must contain ContentBuilder. Steam still authenticates in preview mode. Use a dedicated test depot and do not run another upload against the same SDK concurrently.

For itch.io set `BUILDUPLOADER_TEST_BUTLER` to the full native Butler executable path. No itch.io credentials are required for dry run.

For Unity 6 set `BUILDUPLOADER_TEST_BUILD_PROFILE` to a Build Profile asset matching the active target, with an enabled scene. For older editors configure enabled scenes and Player Settings. Launch the editor with the target already selected; switching targets during a test can reload assemblies.

Tests temporarily configure SDK preferences and restore them afterward. UploadTask performs its normal report saving and build/upload counter updates. Use dedicated test projects. Outputs are retained under `TestResults/BuildUploader`; normal task caches follow package preferences.

## Matrix

The package minimum is Unity 2019.4. Unity 6-only features remain version guarded. Sources have been compiler checked against 2019.4.41f2, 2022.3.62f3, 6000.3.21f1 and 6000.7.0a2; this does not establish runtime or host coverage for every combination.

Use a separate compatible Unity project per editor version, with its matching Test Framework. Do not downgrade the Unity 6 project. Install the target modules and any toolchain required by each profile. Windows, macOS and Linux hosts use the same test sources.

Copy `matrix.example.json`, configure editor/project paths and profiles, then run:

```
python UnitTest/run_matrix.py my-matrix.json --output /absolute/path/to/fresh-results
```

The runner executes service tests once per editor and a separate process for each configured desktop target. NUnit XML, editor logs and `matrix.json` record results. Missing configuration produces skips, and an incomplete matrix returns a nonzero exit code. Compiler checks are not counted as executed tests.
