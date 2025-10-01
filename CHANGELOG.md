# 3.0.1

## What's new
- Added preferences toggle to auto show Upload Task Window when starting an upload
- Added `Remove all Build Configs` to Build Configs settings context menu
- Added {buildArchitecture} string format
- Added error messages if selected platforms aren't installed or supported

## What's fixed
- Fixed exception spam when viewing build configs in project settings
- Fixed unable to make builds in some Unity versions
- Fixed able to start an upload using platforms that aren't installed or supported
- Fixed obsolete platforms showing in dropdowns
- Fixed sometimes platforms defaulting to windows server - not windows player
- Fixed upload validation failing due to starting an upload that contains build configs
- Fixed upload task continuing to make multiple builds of previous have failed. (early out)
- Fixed unable to make WebGL builds if Unity isn't already set to WebGL
- Fixed WebGL builds not including build meta data
- Fixed default build configs not having any scenes in them (now adds current open scene)
- Fixed Task window not expanding to show all possible text for a step (still cuts off if over 32k character)
- Fixed builds failing if deep profile is enable but development build is disabled


# 3.0.0

## Highlights
  - Create multiple builds and upload them with 1 button
  - Upload builds to itch.io
  - Post a message to Discord using an App (Bot)
  - New Upload Task window to view old and in-progress tasks and their logs
  - Lots of Steamworks destination fixes (No more 2 factor per upload!)
  - Lots of UI and UX improvements

## What's new
  - Added BuildConfigs (Define all Editor and Player settings that you want in a build)
  - Added Itch.io upload destination
  - Added Post upload actions
  - Added Discord app messaging action (Can send messages to discord on successful upload)
  - Added Upload Profiles (Can name multiple sets of uploads and change between them)
  - Replaced Report window with new Task window to view in progress tasks and their logs `Window/Build Uploader/Open Upload Tasks Window`
  - Added BuildConfig source (Start a new build using the config when uploading)
  - Added BuildData to every build's StreamingAssets to see its build number (can be opted out in ProjectSettings)
  - Build Uploader now makes a ProjectID.txt to differentiate between different projects when using EditorPrefs
  - New string formats
    - {buildName} - The name of the build as specified in a build config.
    - {buildNumber} - The unique number of the build that is produced if enabled in Project Settings.
    - {buildTarget} - The target group of the upcoming build as defined in Player Settings.
    - {buildTargetGroup} - The target group of the upcoming build as defined in Player Settings.
    - {cacheFolderPath} - The path where all files and builds are stored when build uploader is working.
    - {machineName} - The name of the machine running the build.
    - {persistentDataPath} - The version of your project as specified in Player Settings.
    - {projectPath} - The version of your project as specified in Player Settings.
    - {productName} - The name of your product as specified in Player Settings. (Previously projectName)
    - {scriptingBackend} - The scripting backend for the next build as defined in Player Settings.
    - {taskFailedReasons} - Gets the reasons why the task failed to upload all destinations.
    - {taskProfileName} - The name of the upload profile or task specified when creating the task.

## What's changed
  - General
    - Removed Build button in the upload window in favor of using build configs.
    - Upload profiles / upload configs are saved per project and can sync with source control
    - Upload tab no longer shows percentage of upload
    - Added settings context options to reorder upload configs
    - Added foldout and right-click context options to all reorderable lists
    - Can now remove Steamworks Configs
    - Added `.*ButDontShipItWithYourGame` when creating a new folder regex modifier for IL2CPP builds
    - UI changes to Project settings and Preferences
  - Upload Tasks
    - API changed to allow for more customisability
    - Added Validation step to prevent starting an upload via CI if there are errors
    - Added PrepareSource step for caching before starting to make builds and modify editor
    - More logs added to assist debugging
    - Cleanup step is now async
    - Cleanup will now happen even if Preferences is set to not clear cache after upload
  - Unity Cloud
    - Tab moved to its own window `Window/Build Uploader/Open Unity Cloud Window`
    - CloudBuildManifest no longer makes a new UnityCloudBuildManifest.json in builds if it doesn't exist
  - Welcome Window
    - Changelog shows dropdowns per versions instead of everything all at once

## What's fixed
  - String formats
    - Fixed unable to use taskDescription and other strings in destinations
    - Fixed dropdowns not formatting (eg: Source type, steam depot...)
  - LocalPathDestination
    - Fixed show button not going to the path if its formatted
    - Fixed Duplicate File Handling not save/loading
    - Added error check for invalid characters in path 
  - Steamworks destination
    - Fixed uploading multiple builds to multiple depots on steam resulting in the same build being uploaded
    - Fixed first time uploading to steamworks without a branch throwing an error
    - Fixed changing Steam DRM toggle and flags from not auto-saving
    - Possible fix for Steam DRM wrapping the wrong .exe depending on Product name
  - Github
    - Fixed draft and prerelease always true when uploading
  - Other
    - Fixed some JSON deserialization edge cases
    - Fixed window icons not showing when the package is added to the Assets folder.


# 2.4.1
- Upload tab
  - Added pre-upload error reporting when Exclude files/folders has bad regex
  - Added warning when cache file path exceeds OS limit (Windows 260, Mac 255, Linux 4096 characters)
  - Added string format examples  to "Show as Formatted text" toggle
  - Fixed new modifiers starting with bad regex 
  - Fixed string formatting not working in most sources, modifiers, destinations
  - Fixed file/folder source text field not filling the entire container
- Preferences
  - Fixed cache build folder size not updating when changing the path


# 2.4.0
- General
  - Added menu item to open the build task report window `Window/Build Uploader/Build Task Report`
  - Welcome Window changelog text is now showing formatted text
- Upload Tab
  - Added `Build and Upload All` button
  - Added `LastBuildDirectory` source that chooses the directory of the last build made using the Build Uploader
  - Added button to start a new unity build and upload if successful
  - Fixed warning and error message UI double ups
  - Fixed errors and warnings showing when source/modifier/destination is disabled
- Exclude FIle/Folder Modifiers
  - Added `When To Exclude` dropdown to choose when to exclude files/folders. 
    - Set to `DoNotCopyFromSource` by default.
    - Existing modifiers will migrate to `DeleteFromCache`
- Steam Destination
  - Fixed uploading to multiple branches on Steam only appearing on 1
- LocalPathDestination
  - Can now use string formatting to change the file name and path
    - See [docs](https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/StringFormatter) for all formats you can use
  - Fixed non-existent path showing as an error and now shows as a yellow warning
- Preferences
  - Added toggle to toggle auto saving of build config changes
  - Added dropdown in preferences to change when confirmation popups show
  - Added dropdown in preferences to change when build task report shows
  - Added field in preferences to change what the default description is in the upload tab
    - See [docs](https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/StringFormatter) for all formats you can use
  - Now shows size of the cached builds folder

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
