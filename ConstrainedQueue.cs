using System.Collections.Generic;
using Validation;

namespace ThrottledRestClient
{
    internal sealed class ConstrainedQueue<T>
    {
        private readonly int _capacity;
        private readonly Queue<T> _queue;

        internal ConstrainedQueue(int capacity)
        {
            Requires.Range(capacity > 0, nameof(capacity));

            this._capacity = capacity;
            this._queue = new Queue<T>();
        }

        internal bool IsFull => this._queue.Count == this._capacity;

        internal void Enqueue(T item)
        {
            this._queue.Enqueue(item);
            if (this._queue.Count > this._capacity)
            {
                this._queue.Dequeue();
            }
        }

        internal T Peek()
        {
            return this._queue.Peek();
        }
    }
}