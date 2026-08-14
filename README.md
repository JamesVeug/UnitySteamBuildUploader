<p align="center"><img src="https://github.com/JamesVeug/UnitySteamBuildUploader/blob/main/Assets/LargeIcon.png?raw=true" alt="MAS Logo"></p>

<h1 align="center">Build Uploader</h1>

<p align="center">Unity Editor Tool For Making Builds, Uploading Them To Online Services And Notifying Chat Apps</p>

<p align="center">Steamworks | Itch.io | Epic Games | Github | Discord | Slack</p>

<p align="center">Open Source | For Commercial Use</p>

<hr>

[![LICENSE](https://img.shields.io/github/license/JamesVeug/UnitySteamBuildUploader)](LICENSE)
[![STARS](https://img.shields.io/github/stars/JamesVeug/UnitySteamBuildUploader)](https://github.com/JamesVeug/UnitySteamBuildUploader)
[![Github All Releases](https://img.shields.io/github/downloads/JamesVeug/UnitySteamBuildUploader/total.svg)]()

## Key Points 💡
- Windows, Mac, Linux support
- Unity Supported (Light and Dark)
  - Unity 6000.1-5
  - Unity 202x
  - Unity 2020 and 2019
    - Requires package [com.unity.sharp-zip-lib](https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/Installation.html)
- Advanced GUI and non-GUI support (CI/CD)
  - Drive a running Editor from a terminal, CI or an AI agent with Unity's `unity command` CLI
- Create multiple builds using Build Configs or Unity's Build Profiles.
  - Windows, Mac, Linux, Webgl (Others loosely supported via Unity's Build Profiles)
- Services
  - Steamworks
    - Uploading a build to any branch or depots
    - DRM wrap (anti-piracy)
  - Itch.io
    - Uploading a build
  - Epic Games
    - Upload builds to specific artifacts
  - Github
    - Upload a new Release
  - Discord
    - Send message to a channel
  - Slack
    - Send message to a channel
  - Unity Cloud Build
    - View, download and start builds
- Safely modify builds before uploading
  - Remove files/folders
  - Compress/Decompress files
  - DRM wrap (anti-piracy)
- Minimal build size impact
- For personal and commercial use
- Open Source


## Unity CLI (`unity command`) 🖥️

If you have Unity's [Pipeline package](https://github.com/Unity-Technologies/com.unity.pipeline) (`com.unity.pipeline`) installed, the Build Uploader registers a `build_uploader` command that lets you drive an **already-open Editor** from a terminal, a CI script or an AI agent — no `-batchmode` restart needed.

```bash
unity command build_uploader --profiles true
```

The package is completely optional. If `com.unity.pipeline` is not installed nothing changes, and the command simply isn't there.

Everything is exposed through the single `build_uploader` command, with one argument per operation. Arguments that take profiles accept a GUID *or* a profile name, arguments that take tasks accept a task GUID, and all of them accept `all` — values can be comma- or space-separated.

**Look at what's there**

| Argument | Does |
|---|---|
| `--profiles` | Lists every upload profile (name and GUID) |
| `--active_tasks` | Lists uploads that are currently running or queued |
| `--source_types` / `--modifier_types` / `--destination_types` / `--action_types` | Lists everything you can pick in the GUI, with its description |
| `--reports` | Lists saved upload reports — `all`, or filtered by profile |
| `--cache_summary` | Cache folder path, size, free disk space, cached builds and saved reports |

**Check it before you run it**

| Argument | Does |
|---|---|
| `--verify_profiles` | Runs the same validation the Validation step runs — errors and warnings, without fetching a source or contacting a service |
| `--summarize_profiles` | Prints each profile's sources, modifiers, destinations and actions |
| `--verify_tasks` / `--summarize_tasks` | Pass/fail and per-step progress for a past or running upload |
| `--open_tasks` | Prints a task's full report. Add `--errors_only` for just the failed steps |

**Run it**

| Argument | Does |
|---|---|
| `--start_tasks` | Runs the full pipeline for a profile |
| `--dry_run_tasks` | Runs everything with every destination swapped for *Nowhere*, so nothing is uploaded |
| `--cancel_tasks` | Cancels a running upload |

Uploads start asynchronously so the Editor stays responsive — poll `--active_tasks` and then read the result with `--summarize_tasks` or `--open_tasks`.

**Maintain it**

| Argument | Does |
|---|---|
| `--clone_profiles` | Duplicates a profile. Add `--new_name` to name the copy |
| `--clone_tasks` | Re-runs a task from this Editor session with its exact config |
| `--delete_profiles` / `--delete_tasks` | Deletes a profile, or a task's saved report and cached build |
| `--clear_cache` | Empties the cache folder, keeping your saved reports |

Anything that deletes needs `--confirm` as well, otherwise it is refused:

```bash
unity command build_uploader --clear_cache true --confirm true
```

A typical agent or CI flow is: verify, dry run, then upload.

```bash
unity command build_uploader --verify_profiles "Release Build"
unity command build_uploader --dry_run_tasks "Release Build"
unity command build_uploader --start_tasks "Release Build"
unity command build_uploader --active_tasks true
```

Full argument reference: [CLI](https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/CLI).

Prefer a fully headless run with no Editor open? See [Starting a BuildTask without UI](https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/Starting-a-BuildTask-without-UI).


## Wiki
- Home: https://github.com/JamesVeug/UnitySteamBuildUploader/wiki
- How to Install: https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/How-to-Install
- How to Setup: https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/How-to-Setup
- How does it Work: https://github.com/JamesVeug/UnitySteamBuildUploader/wiki#how-does-it-work
- CLI (`unity command`): https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/CLI


## Links 
- Support Me: https://buymeacoffee.com/jamesgamesnz
- Discord: https://discord.gg/R2UjXB6pQ8
- Github: https://github.com/JamesVeug/UnitySteamBuildUploader
- Asset Store: https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907



## Security 🔒
This package does NOT distribute any personal information. Any information entered is encrypted locally on your computer.

Keeping your credentials safe is important to me!

If you discover any security related issues, please email me, message on discord or create an issue on [github.](https://github.com/JamesVeug/UnitySteamBuildUploader)


## Reporting bugs / suggesting changes ❓

If you find a bug or want to suggest a change, [create an issue on github](https://github.com/JamesVeug/UnitySteamBuildUploader/issues)

Include:
- What the problem is
- What you expected to happen
- What version of Build Uploader you are using
- What version of Unity you are using
- Are you using Windows/Linux/Mac


## How to Contribute 🔨
- Fork the [repository](https://github.com/JamesVeug/UnitySteamBuildUploader)
- Make your changes
- Create a pull request to the `develop` branch
  - Include detailed description of the changes you made and why
  - Include what versions of Unity you tested it on
  - Include any concerns with the changes you made (So i'm aware of them too)


## How to Support 🙏

A lot of effort has been put into this package for others to freely use. Any kind of support is greatly appreciated and encourages further work!

- ⭐ Star the [Github repository](https://github.com/JamesVeug/UnitySteamBuildUploader)
- ✍️ Review on the [Asset Store](https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907)
- ☕ Buy me a coffee: [buymeacoffee](https://buymeacoffee.com/jamesgamesnz)
- 💬 Report bug reports or suggestions improvements: [Github Issues](https://github.com/JamesVeug/UnitySteamBuildUploader/issues)
- 🔗 Share the package with your friends and colleagues

### Supporters

- [Nementic Games](https://store.steampowered.com/developer/nementic/)
- [Classy Games](https://store.steampowered.com/developer/classygames)

## License 🪪
Creative Commons - CC0 1.0 Universal

Use this package however you want - commercially or non-commercially.


## Created with AI
AI has assisted by providing code suggestions.

All suggestions have been reviewed and rewritten by a Senior Unity Developer to fit the package's requirements.