<p align="center"><img src="https://github.com/JamesVeug/UnitySteamBuildUploader/blob/main/LargeIcon.png?raw=true" alt="MAS Logo"></p>

<h1 align="center">Build Uploader</h1>

<p align="center">Unity Editor Tool For Uploading Builds To Online Services.</p>

<p align="center">Steamworks Support | Open Source | Commercial Use</p>

<hr>

[![LICENSE](https://img.shields.io/github/license/JamesVeug/UnitySteamBuildUploader)](LICENSE)
[![STARS](https://img.shields.io/github/stars/JamesVeug/UnitySteamBuildUploader)](https://github.com/JamesVeug/UnitySteamBuildUploader)

## Key Points 💡
- Windows
- Unity 2021 and above.
  - No package dependencies
- Unity 2020 and below.
  - Requires package [com.unity.sharp-zip-lib](https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/Installation.html)
- Services
  - Steamworks
    - Uploading a build
    - DRM Wrapping
  - Unity Cloud Build
    - View builds
    - Download builds
    - Start new builds
- Minimal build size impact
- Can be used commercially
- Open Source


## Links 
- Support Me: https://buymeacoffee.com/jamesgamesnz
- Discord: https://discord.gg/R2UjXB6pQ8
- Github: https://github.com/JamesVeug/UnitySteamBuildUploader
- Asset Store: https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907


## How to Install 💿

<a href="https://youtu.be/w_ffKFQ5nh4?si=_bk7xMUItqdL1uUn"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/90/Logo_of_YouTube_%282013-2015%29.svg/2560px-Logo_of_YouTube_%282013-2015%29.svg.png" alt="Git-Sync-Tab-Pic" border="0" width="81" height="32"></a>


### 1. Add package to your project

#### Unity Asset Store
- https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907
- Add to your assets/download
- Add to your project

#### Manually
- Open `Window->Package Manager`
- Press the `+` button in the top left
- Choose `Add package from git URL`
- Enter the git url `https://github.com/JamesVeug/UnitySteamBuildUploader.git`
- Press `Add`
- Unity 2020 and below
  - Repeat above steps with but using `com.unity.sharp-zip-lib`

### 2. Setup

<a href="https://imgur.com/CSyR6M4"><img src="https://i.imgur.com/CSyR6M4.png" alt="Screenshot-2025-01-03-213527" border="0"></a>

- a. Go to `Edit->Preferences->Build Uploader`
  - Steamworks
    - Turn on Enabled
    - [Download SteamSDK](https://partner.steamgames.com/downloads/list) and extract it to your computer
      - Enter the path to the SteamSDK folder
    - Enter your Steamworks login details
  - Unity Cloud Build (Optional)
    - Turn on Enabled
    - Enter your Organization 
      - You use this to log into https://cloud.unity.com/home/login
    - Enter your Project ID
      - You can find this in the URL of your Unity Cloud Build page eg: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    - Enter the DevOps API Key
      - This is found in `DevOps->Settings->API Key`
      
<a href="https://imgur.com/QxMZ09b"><img src="https://imgur.com/QxMZ09b.png" alt="Screenshot-2025-01-03-212949" border="0"></a>

- b. Go to `Edit->ProjectSettings->Build Uploader`
  - Press `New`
  - Enter the name of your game
  - Enter the AppID of your game. (Found in the URL of your games store page. eg: `1141030`)
  - Press `+` to create a new depot for your game
    - Enter a name for the depot
    - Enter the depot ID (Found in the Steamworks website eg: `1141031`)
  - Add any branches that you need (`none` is also known as default on Steamworks)

- c. Go to `Window->Build Uploader->Build Configs`
  - Go to Upload Tab
  - Press `New` to create a Build Config
  - Choose where your build will come from (AKA: Source)
    - Choose `Folder` if you have already build game or DLC pack you want to upload
    - Choose `File` if you have a .zip file you want to upload
    - Choose `UnityCloud` if you want to Upload a build from Unity Cloud
  - Choose where your build will go (AKA: Destination)
    - Choose `Steamworks` if you want to upload to Steam
      - Choose which Game you want to upload it to
      - Choose Depot
      - Choose Branch
    - Choose `Nowhere` to test retrieving your Build and modifying it without uploading it anywhere
      - You can view where the modified build is in the Cache folder go going to `Edit->Preferences->Build Uploader`
  - Optional
    - Press `>` to see more details about what you're uploading and where from
    - Under Modifiers, you can make any additional changes
  - Press `Save`


### 3. Upload

- Go to `Window->Build Uploader`
- Go to Upload Tab
  - NOTE: If you have no Builds Configs setup yet see #2c
- Enable each of the Build Configs you want to upload (Checkbox at the beginning)
- Enter description of the build (eg: `v1.0.1 - Hotfix to fix jumping bug`)
- Press `Download and Upload all`
  - If this button does not show then there will be errors mentioned in the giant button at the bottom. Fix them and repeat.

> !NOTE: If you have no Builds Configs setup yet see #2c


## Upload Tab
<a href="https://imgur.com/pBnYzJR"><img src="https://imgur.com/pBnYzJR.png" alt="Git-Sync-Tab-Pic" border="0"></a>

Specify where you want builds to come from and where you want them to go.
- You can specify a file/file on your computer or choose from your Unity Cloud builds.
- Choose where you want the build to go (Steam depot & branch)
- Add any additional modifiers to change the build before uploading
- Set a description to appear on steam
- Click Download and Upload all

**NOTE: You can not upload to the default branch (default branch everyone uses). This is on purpose to avoid uploading the wrong build, Also the SDK does not allow this.**


## Unity Cloud Tab (Optional)
<a href="https://imgur.com/PNFJd87"><img src="https://imgur.com/PNFJd87.png" alt="Git-Unity-Cloud-Pic" border="0"></a>

Utilize Unity Cloud to automate make builds of your project.
- Tracks progress of current builds
- Start a new build
- Download builds

## How does it work?

<a href="https://imgur.com/3cKv2zs"><img src="https://i.imgur.com/3cKv2zs.png" alt="Upload process" border="0"></a>

When pressing the `Download and Upload all` button the editor starts an async Build Task to begin the upload process.

The Build Task has a number of steps that it goes through sequentially while its uploading. Each step is async and fires for each Build Config at the same time. When a Step is complete for all Build Configs it will proceed to the next step.

> NOTE: If any Build Config fails at any point then the whole build task will cancel and notify the user of the issue.

### 1. Get Source

Download the build from the selected source
eg: Choose File/Folder will save that location
eg: Choose Unity Cloud will download the build from Unity Cloud


### 2. Cache Source

This will copy the selected contents from Source to a cache folder.

`%userprofile%/appdata/locallow/<companyname>/<productname>/BuildUploader/CachedBuilds`

The copied contents are required to be a folder and not a file or zip. The build step will unzip the contents.

### 3. Modify Cached Contents

This is where modifiers are applied to the cached contents.
- Remove DoNotShip files/folders
- Apply Steam DRM
- etc

### 4. Upload Contents

This is the final step where everything is expected to be valid and ready to upload to the selected Destination.

Each build config will upload at the same time.

> NOTE: If 1 build fails to upload it does NOT prevent the others from uploading.
> 
> Example: Steam has the incorrect login credentials.
> 
> You can test the build before uploading by selecting `Nowhere` as the destination and disabling Deleting Caching in preferences. 
> This will still go through the same process but not upload the build anywhere.

### 5. Cleanup

After the upload is complete the cache folder is deleted (Unless specified in Preferences to not)



## Security 🔒
This package does NOT distribute any personal information. Any information entered is stored locally on your computer and not in plain text format.

Keeping your credentials safe is important to me!

If you discover any security related issues, please email me, message on discord or create an issue on [github.](https://github.com/JamesVeug/UnitySteamBuildUploader)


## Known Issues 🪳
- Steam Two-Factor authentication requires you to enter the steam guard code manually every time you upload
  - I Don't know why pressing "Confirm that's me" doesn't work, and it does not cache this locally like the Steam Guard Code.

## Reporting bugs / suggesting changes ❓

If you find a bug or want to suggest a change, please create an issue on the [github page](https://github.com/JamesVeug/UnitySteamBuildUploader).

Include:
- What the problem is
- What you expected to happen
- What version of Build Uploader you're using
- Are you using Mac or Windows


## How to Contribute 🔨
- Fork the [repository](https://github.com/JamesVeug/UnitySteamBuildUploader)
- Make your changes
- Create a pull request to the `develop` branch
  - Include detailed description of the changes you made and why
  - Include what version of Unity you tested it on
  - Include any concerns with the changes you made (So i'm aware of them too)


## How to Support 🙏

A lot of effort has been put into this package for others to freely use. Any kind of support is greatly appreciated and encourages further work!

- Star ⭐ the [Github repository](https://github.com/JamesVeug/UnitySteamBuildUploader)
- Review this package on the [Asset Store](https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907)
- Buy me a coffee: https://buymeacoffee.com/jamesgamesnz

## License 🪪
Creative Commons - CC0 1.0 Universal

Use this package however you want - commercially or non-commercially.