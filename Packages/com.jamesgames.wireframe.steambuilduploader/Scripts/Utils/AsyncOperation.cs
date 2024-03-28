using System.Collections;

namespace Wireframe
{
    public class AsyncOperation<T> : AsyncOperation
    {
        public T Data { get; set; }
    }

    public class AsyncOperation : IEnumerator
    {
        public bool Successful { get; set; }

        private IEnumerator iterator;

        public void SetIterator(IEnumerator iterator)
        {
            this.iterator = iterator;
        }

        public bool MoveNext()
        {
            return iterator.MoveNext();
        }

        public void Reset()
        {
            iterator.Reset();
        }

        public object Current => iterator.Current;
    }
}