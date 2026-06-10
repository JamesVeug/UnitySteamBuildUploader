using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class XboxConfig
    {
        [SerializeField]
        public List<XboxApp> apps;

        public void Initialize()
        {
            apps = new List<XboxApp>(1);
        }
    }
}
