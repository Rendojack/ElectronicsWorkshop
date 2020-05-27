using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.DataStructures
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        private readonly object _lock = new object();
        public int Size { get; private set; }

        public FixedSizeQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (_lock)
            {
                while (base.Count > Size)
                {
                    base.Dequeue();
                }
            }
        }
    }
}
