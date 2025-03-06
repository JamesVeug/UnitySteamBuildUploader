using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal abstract class ABuildConfigModifer
    {
        public abstract void Setup(Action onChanged);
        public abstract Task<UploadResult> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex);
        public abstract bool OnGUI();
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}