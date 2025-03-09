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
