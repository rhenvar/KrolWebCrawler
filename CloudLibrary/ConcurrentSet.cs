using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public class ConcurrentSet<T>
    {
        private ConcurrentDictionary<T, int> dictionary;
        public int Size { get; private set; }

        public ConcurrentSet()
        {
            dictionary = new ConcurrentDictionary<T, int>();
            Size = 0;
        }

        public bool Contains(T t)
        {
            return dictionary.ContainsKey(t);
        }

        public void Add(T t)
        {
            if (!Contains(t))
            {
                dictionary.TryAdd(t, 0);
                Size++;
            }
        }
    }
}