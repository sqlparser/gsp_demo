using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gudusoft.gsqlparser.demos.util
{
    public class LinkedHashMap<T, U>
    {
        Dictionary<T, LinkedListNode<Tuple<U, T>>> D = new Dictionary<T, LinkedListNode<Tuple<U, T>>>();
        LinkedList<Tuple<U, T>> LL = new LinkedList<Tuple<U, T>>();

        public U this[T c]
        {
            get
            {
                return D[c].Value.Item1;
            }

            set
            {
                if (D.ContainsKey(c))
                {
                    LL.Remove(D[c]);
                }

                D[c] = new LinkedListNode<Tuple<U, T>>(Tuple.Create(value, c));
                LL.AddLast(D[c]);
            }
        }

        public bool ContainsKey(T k)
        {
            return D.ContainsKey(k);
        }

        public bool Remove(T k) {
            if (D.ContainsKey(k))
            {
                LL.Remove(D[k]);
                D.Remove(k);
                return true;
            }
            return false;
        }

        public void Clear() {
            D.Clear();
            LL.Clear();
        }
        public U PopFirst()
        {
            var node = LL.First;
            LL.Remove(node);
            D.Remove(node.Value.Item2);
            return node.Value.Item1;
        }

        public int Count
        {
            get
            {
                return D.Count;
            }
        }

        public Dictionary<T, LinkedListNode<Tuple<U, T>>>.KeyCollection Keys
        {
            get
            {
                return D.Keys;
            }
        }
    }

}
