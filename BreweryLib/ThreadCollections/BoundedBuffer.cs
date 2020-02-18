using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BreweryLib.ThreadCollections
{
    public class BoundedBuffer<T>
    {
        private Mutex _mutex;
        private Semaphore _full;
        private Semaphore _empty;
        private Queue<T> _buffer;

        public BoundedBuffer(int capacity = 10000000)
        {
            _mutex = new Mutex();
            _full = new Semaphore(capacity, capacity);
            _empty = new Semaphore(0, capacity);
            _buffer = new Queue<T>(capacity);
        }

        public void Insert(T item)
        {
            _full.WaitOne();
            _mutex.WaitOne();
            _buffer.Enqueue(item);
            _mutex.ReleaseMutex();
            _empty.Release();
        }

        public T Take()
        {
            _empty.WaitOne();
            _mutex.WaitOne();
            T item = _buffer.Dequeue();
            _mutex.ReleaseMutex();
            _full.Release();

            return item;
        }

        public int Peek()
        {
            return _buffer.Count;
        }
    }
}
