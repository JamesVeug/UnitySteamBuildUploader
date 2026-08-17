using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public partial class EmailConfig
    {
        [SerializeField]
        public List<EmailAccount> accounts;

        public void Initialize()
        {
            accounts = new List<EmailAccount>(2);
        }
    }
}
