using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class EMailConfig
    {
        [SerializeField]
        public List<EMailAccount> accounts;

        public void Initialize()
        {
            accounts = new List<EMailAccount>(2);
        }
    }
}
