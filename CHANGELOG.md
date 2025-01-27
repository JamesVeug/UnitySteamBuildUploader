# 1.2.2
- General
  - Dropdowns now appear in order of ID
  - Renamed `Manual` to `File` in the Upload tab
  - Display Window will now appear when failing to upload logs
  - Fixed steam branches not save/loading
  - Fixed some null refs
  - Obfuscated steam username in logs
- Steam
  - Added missing icon for SteamGuard windows
  - Improved logs

# 1.2.1
- General
  - All editor based classes are now internal
  - All classes now have a namespace
  - Fixed deserializing objects with missing fields
- UnityCloud tab
  - Fixed builds not showing up
  - Added refresh button to sync configs
  - Better error messaging when failing to get builds from the cloud
- Project Settings
  - Changed UnityCloud labels to be more representative of what they need

# 1.2.0
- General
  - Removed `com.unity.nuget.newtonsoft-json` dependency in favor of internal solution.
  - Tabs (Steamworks/UnityCloud) can now be enabled/hidden in `Preferences/Steam Build Uploader`
  - Fixed dropdowns not showing duplicate entries
  - Fixed trying to type your game name twice, and it disappears from the list
  - Fixed pressing `+` to create a new config not showing correct depots and branches.
- Steamworks tab
  - Moved to `ProjectSettings/Steam Build Uploader`
- Unity Cloud tab
  - Disabled/Hidden by default. Enable in `Preferences/Steam Build Uploader`.

# 1.1.2
- General
  - Removed EditorCoroutine dependency
  - Fixed CloudBuildManifest breaking builds
- Upload tab
  - Fixed uploading occuring more than once
  - Fixed Sharing violation exception when uploading a build to a new depot 
- Steam
  - Added window to verify Steam Guard during uploads. **NOTE: Two-Factor authentication does not save between sessions. Don't know why.**
  - Added more errors logs indicating the reason an upload failed
- Preferences
  - Fixed Steam credentials hiding/showing when entering password
  - Tidy up of UI

# 1.1.1
- General
  - New configs start with none instead of default
  - CloudBuildManifest can now be used in builds
- Steam tab
  - Removed Description, IsPreviewBuild and LocalContent server. They did nothing.
  - Any failed steps now stop attempting to upload
  - Changing upload settings while uploading no longer changes the existing upload sequence.

# 1.1.0
- General
  - Moved all login credentials to `Edit->Preferences->Steam Build Uploader`
  - Added icon to the window
  - Logging is a little better
- Steam tab
  - Changed lists of depots and branches to be reorderable
  - Changed default branch to be called "none" instead of "default"
  - Added button to open steam sdk console
  - Added button in configs to open the games steam store page
  - Added button to open steam sdk page to download it if your path is not valid
- Unity Cloud tab
  - Fix for macos builds spamming errors
  - Builds older than 1 year now show the exact years
- Upload tab
  - Default source type changed to Manual
  - Manually uploads now require a `.exe` or `.zip` in order to upload. `.zip` will be unzipped when uploading.
  - Builds will no longer upload if any sources failed to provide a valid path 

# 1.0.1
- Replaced EditorCoroutine with Tasks (You can remove that dependency package now)
- Hid StreamSDK and UnityCloud credentials
- Removed Upload tab and renamed Sync tab to Upload
- Upload tab now shows `*` on the save button if something has changed.
- Added error text to the `Download and Upload` button to indicate what went wrong
- Manual upload file now goes red when file does not exist
- Changed Expand button to `>` and `\/` instead of `+` `-`
- Added more information to background tasks.
- Fixed bugs so the package works with 2020.x
- Fixed Creating new steam configs not showing in the dropdown
- Fixed Steam depot dropdown not refreshing when adding a new depot
- Fixed manual builds not deleting their cached files in cleanup
- Fixed upload configs not save/loading between sessions
- Fixed manual upload source not saving the file path

# 1.0.0
- Initial version