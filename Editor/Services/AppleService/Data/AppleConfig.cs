using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class AppleConfig
    {
        [SerializeField]
        public List<AppleApp> apps;

        [SerializeField]
        public List<AppleApiKey> apiKeys;

        public void Initialize()
        {
            apps = new List<AppleApp>(2);
            apiKeys = new List<AppleApiKey>(2);
        }
    }
}
