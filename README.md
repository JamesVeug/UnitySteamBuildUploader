# Steam Build Uploader

This package is designed to help automate the process of uploading builds to Steam. It is designed to work with the Unity Cloud Build system and the Steam SDK.


## Setup
- Add the package to your project in package manager using giturl `https://github.com/JamesVeug/UnitySteamBuildUploader.git`
- Add the `editorcoroutine` package to your project
- Add the `newtonsoft package` to your project https://github.com/applejag/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM
- Go to the SteamSDK tab and setup your game




## SteamSDK Tab
![Alt Text](https://raw.githubusercontent.com/JamesVeug/UnitySteamBuildUploader/main/Git_SteamSDKPic.png)

Utilize the SteamSDK to connect to steam. Setup your individual games with the repos and branches you want to upload to.

Download and extract the SteamSDK to a folder on your computer: https://partner.steamgames.com/doc/sdk


## Unity Cloud Tab
![Alt Text](https://github.com/JamesVeug/UnitySteamBuildUploader/blob/main/Git_UnityCloudPic.png?raw=true)

Utilize Unity Cloud to automate make builds of your project.
- Tracks progress of current builds
- Start a new build
- Download builds


## Sync Tab
![Alt Text](https://github.com/JamesVeug/UnitySteamBuildUploader/blob/main/Git_SyncTabPic.png?raw=true)

Specify where you want builds to come from adn where you want them to go.
- You can specify a file on your computer or choose from your UnityCloud builds.
- Choose where you want the build to go (Steam depot & branch)
- Set a description to appear on steam
- Click Downlaod and Upload all

**NOTE: You can not upload to the none branch (default branch everyone uses). This is on purpose to avoid uploading the wrong build. Also the SKD does not allow this.**
