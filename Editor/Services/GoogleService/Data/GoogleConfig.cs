using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class GoogleConfig
    {
        [SerializeField]
        public List<GoogleApp> apps;

        [SerializeField]
        public List<GoogleDriveFolder> driveFolders;

        [SerializeField]
        public List<GoogleChatSpace> chatSpaces;

        [SerializeField]
        public List<GooglePlayApp> playApps;

        public void Initialize()
        {
            apps = new List<GoogleApp>(2);
            driveFolders = new List<GoogleDriveFolder>(2);
            chatSpaces = new List<GoogleChatSpace>(2);
            playApps = new List<GooglePlayApp>(2);
        }
    }
}
