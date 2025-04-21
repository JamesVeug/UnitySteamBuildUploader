using System.Threading.Tasks;

namespace Wireframe
{
    public abstract class ABuildTask_Step
    {
        public abstract string Name { get; }
        public abstract Task<bool> Run(BuildTask buildTask);
        public abstract void Failed(BuildTask buildTask);
    }
}