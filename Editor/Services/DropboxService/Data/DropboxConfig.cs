using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class DropboxConfig
    {
        [SerializeField]
        public List<DropboxApp> apps;

        [SerializeField]
        public List<DropboxFolder> folders;

        public void Initialize()
        {
            apps = new List<DropboxApp>(2);
            folders = new List<DropboxFolder>(2);
        }
    }
}
