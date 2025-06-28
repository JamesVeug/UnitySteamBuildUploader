# 2.3.0
- General
  - Sources/Destinations/Modifiers show errors/warnings where on the build config instead of just on the button   
  - Steam Destination 
    - Uploading multiple builds at once now happens sequentially now due to SteamSDK limitations
    - Can no longer upload to `default` branch due to SteamSDK limitations
    - Added more log handling to show errors from uploading
- Build Report
  - Shows display name of source type when showing files
- Fixes
  - Fixed Upload process not completing when the task completes.
  - Fixed Uploading to Steam with 2 factor saying it failed but actually succeeded.
  - Fixed Build Report not logging to console if the build task fails.
  - Fixed SteamGuard/Two Factor Authentication confirmation popup now showing when uploading multiple builds


# 2.2.0
- General
  - Added **Welcome Window** for more information `Window/Build Uploader/Welcome`
  - Added **Build Report Window** which shows after every build from the UI
  - Exposed a lot of code, so anyone can create and trigger a BuildTask without UI
  - BuildTasks create a BuildTaskReport to easily view logs
  - Made the Description text area bigger and added more options to copy/paste
  - All Sources/Modifiers/Destinations have a `?` to link to their documentation
  - Fixed new lines and tabs not serializing correctly
  - Fixed zipping a folder and saving to the same folder causing IOException
  - Fixed unzipping a .zip file halting editor 
  - Fixed many edge cases causing the BuildTask to run indefinitely and not reporting exceptions
  - Fixed Custom Sources/Modifiers/Destinations made outside the Build Uploader package not showing in the dropdowns
  - Fixed LocalPathDestination requiring a file name when not compressing to a .zip
  - Minor UI improvements
  - Minor Logging improvements
  - Basic [Wiki](https://github.com/JamesVeug/UnitySteamBuildUploader/wiki) support
- Sources
  - Each config can now have multiple Sources
  - Added `URLSource` to download files from online
  - Added `FolderSource`
  - Added PathType to File and Folder Source to select files within the project
  - Changed File Source to only handle files
- Modifiers
  - Can now add/remove any Modifier same as Sources/Destinations
  - Can now be enabled/disabled
  - Added `CompressModifier` to turn a selection of files to a .zip file
  - Added `DecompressModifier` to unzip .zip files
- Destinations
  - Each config can now have multiple Destinations
  - Added `NewGithubRelease` Destination
  - Added Duplicate File dropdown to LocalPathDestination. Set to Overwrite by default
  - Changed SteamworksDestination to create files in the Prepare step to fail before uploading starts
  - Fixed SteamworksDestination not showing Steam Guard popup on first run
- Preferences
  - Added new checkbox to enable/disable auto decompressing .zip files from sources (On by default)
  - Added new checkbox to enable/disable auto saving of build tasks (Off by default)
  - Added new text field to change directory of cached builds.
  - Added tooltips to all preferences
  - Fixed Build Uploader not filtering by certain keywords

# 2.1.1
- General
  - First pass Mac/linux support
  - Fixed DRM Wrapping not working for games/companies with spaces in the name
  - Fixed DRM Wrapping not working with SteamGuard/Two Factor support properly
  - Safety guards to add modifiers back in case they disappear


# 2.1.0
## Upload Tab
- Added `Folder` Source so its now possible ot upload a folder for Steam DLC.
- Added `LocalPath` Destination to copy a modified build to somewhere on your computer.
- Changed `File` Source
  - Only uploads a selected file EXCEPT .exe which will upload the whole folder
  - Removed file extension restriction
- Added build modifiers to change a Build before uploading
  - Remove files meeting regex
    - By default, all build configs remove `*_DoNotShip` files.
  - Steam DRM (Anti-piracy)
    - Upload .exe to Steam so the game has to be run from Steam.
- Description no longer adds `Windows` to what gets uploaded to Steam.
  - Description is exactly what is written in the text box.
- Lots of fixes and improvements to the upload process

## Preferences
- Added Button to go open Cache folder
- Added Toggle to enable/disable removing of cached files after uploading
  - Great for checking what you're uploading before it uploads!
- Fixed trying to enter Steam credentials causing username/password to swap around making it impossible to enter

##  General
- Fixed a bunch of warnings that appear when recompiling the project

## Readme
- Added instructions of how the upload process works
- Lots more information

# 2.0.1
- Fixed issues where the package would not compile in Unity 2020 and 2019

# 2.0.0
- General
  - Package renamed to `Build Uploader`
  - Refactored Steamworks and UnityCloud code be in a services folder
  - Added support me, discord, github links to readme
  - All dropdowns describe what they need instead of something generic
- Fixes
  - Moved all saved directories to a `Build Uploader` folder within `Application.persistentDataPath`
  - Minor UI adjustments to the Upload tab
  - Fixed warnings that appear when recompiling the project
