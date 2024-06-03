using System.Collections;
using System.Threading.Tasks;

namespace Wireframe
{
    public class AsyncOperation<T> : AsyncOperation
    {
        public T Data { get; set; }
    }

    public class AsyncOperation : IEnumerator
    {
        public bool Successful { get; set; }

        private Task iterator;

        public void SetIterator(Task iterator)
        {
            this.iterator = iterator;
        }

        public bool MoveNext()
        {
            return iterator.IsCompleted;
        }

        public void Reset()
        {
            // iterator.Reset();
        }

        public object Current => null;
    }
}