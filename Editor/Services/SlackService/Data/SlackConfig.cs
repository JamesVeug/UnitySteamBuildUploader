using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class SlackConfig 
    {
        [SerializeField]
        public List<SlackServer> servers;
        
        [SerializeField]
        public List<SlackApp> apps;
        
        public void Initialize()
        {
            apps = new List<SlackApp>(2);
            servers = new List<SlackServer>(2);
        }
    }
}