# Steam Build Uploader

This package is designed to help streamline the process of uploading builds to Steam. It is designed to work with the Steam SDK and optionally Unity Cloud.
- No dependencies
- Unity 2020 and above.
- Minimal build size impact
- Open Source




## How to install

<a href="https://youtu.be/w_ffKFQ5nh4?si=_bk7xMUItqdL1uUn"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/90/Logo_of_YouTube_%282013-2015%29.svg/2560px-Logo_of_YouTube_%282013-2015%29.svg.png" alt="Git-Sync-Tab-Pic" border="0" width="81" height="32"></a>


### 1. Add package to your project
- Open `Window->Package Manager`
- Press the `+` button in the top left
- Choose `Add package from git URL`
- Enter the git url `https://github.com/JamesVeug/UnitySteamBuildUploader.git`
- Press `Add`

### 2. Setup

<a href="https://ibb.co/61JHPPn"><img src="https://i.ibb.co/9V3bTT8/Screenshot-2025-01-03-213527.png" alt="Screenshot-2025-01-03-213527" border="0"></a>
- Go to `Edit->Preferences->Steam Build Uploader`
  - Download SteamSDK and extract it to a folder on your computer
    - Enter the path to the SteamSDK folder (https://partner.steamgames.com/downloads/list)
  - Enter your steam login details
  - Enter Unity Cloud details **(Optional)**

<a href="https://ibb.co/9VMYd9p"><img src="https://i.ibb.co/s6B3Xvg/Screenshot-2025-01-03-212949.png" alt="Screenshot-2025-01-03-212949" border="0"></a>
- Go to `Edit->ProjectSettings->Steam Build Uploader`
  - Press `New`
  - Enter the name of your game
  - Enter the AppID of your game. (Found in the URL of your games store page. eg: `1141030`)
  - Press `+` to create a new depot for your game
    - Enter a name for the depot
    - Enter the depot ID (Found in the Steamworks website eg: `1141031`)
  - Add any more branches that you need (`none` is also known as default on steamworks)
- Go to `Window->Steam Build Uploader`
  - Press `New`
  - Choose where your build will come from
    - Choose Manual to choose a file from your computer then select the .zip or .exe of your game
  - Choose where your build will go
    - Choose Manual SteamWorks if you want to upload to steam
    - Choose which game you want to upload it to
    - Choose Depot
    - Choose Branch
  - Press `Save`


### 3. Upload

- Go to `Window->Steam Build Uploader`
  - Select which build you want to upload
  - Choose where you want to upload it to
  - Enter description of the build (eg: `v1.0.1 build 123 - Fixed jumping bug`)
  - Press `Download and Upload all`



## Upload Tab
<a href="https://ibb.co/7RSjdgL"><img src="https://i.ibb.co/3MT49fQ/Git-Sync-Tab-Pic.png" alt="Git-Sync-Tab-Pic" border="0"></a>

Specify where you want builds to come from adn where you want them to go.
- You can specify a file on your computer or choose from your UnityCloud builds.
- Choose where you want the build to go (Steam depot & branch)
- Set a description to appear on steam
- Click Download and Upload all

**NOTE: You can not upload to the default branch (default branch everyone uses). This is on purpose to avoid uploading the wrong build. Also the SDD does not allow this.**


## Unity Cloud Tab (Optional)
<a href="https://ibb.co/6tcrXN3"><img src="https://i.ibb.co/s1pbWt0/Git-Unity-Cloud-Pic.png" alt="Git-Unity-Cloud-Pic" border="0"></a>

Utilize Unity Cloud to automate make builds of your project.
- Tracks progress of current builds
- Start a new build
- Download builds


## Security
This package does NOT distribute any personal information. Any information entered is stored locally on your computer and not in plain text format.

Keeping your credentials safe is important to me!

If you discover any security related issues, please email me, message on discord or create an issue on [github.](https://github.com/JamesVeug/UnitySteamBuildUploader)


## Reporting bugs / suggesting changes

If you find a bug or want to suggest a change, please create an issue on the [github page](https://github.com/JamesVeug/UnitySteamBuildUploader).


## How to contribute
- Fork the [repository](https://github.com/JamesVeug/UnitySteamBuildUploader)
- Make your changes
- Create a pull request to the `develop` branch
  - Include detailed description of the changes you made
  - Include what version of Unity you tested it on
  - Include any concerns with the changes you made

## Known Issues
- Two-Factor authentication does not save between sessions. Don't know why.


## License
Creative Commons - CC0 1.0 Universal

Use this package however you want - commercially or non-commercially.