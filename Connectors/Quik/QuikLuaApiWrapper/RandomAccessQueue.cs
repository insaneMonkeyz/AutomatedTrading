using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace Quik
{
    public delegate bool IsSubjectToDequeue<TEntity, TArg>(TEntity entity, TArg arg);

    public class RandomAccessQueue<T>
    {
        private readonly ReusableLinkedList<T> _queue = new();
        private readonly object _sync = new();

        public bool TryDequeueItem<TArg>(IsSubjectToDequeue<T,TArg> isSubjectToDequeue, TArg arg, out T item)
        {
            lock (_sync)
            {
                var node = _queue.First;
                item = default;

                if (node is null)
                {
                    return false;
                }

                for (int i = 0; i < _queue.Count; i++)
                {
                    if (isSubjectToDequeue(node.Value, arg))
                    {
                        item = node.Value;
                        _queue.Remove(node);
                        return true;
                    }
                }
            }

            return false;
        }
        public bool TryDequeue(out T item)
        {
            lock (_sync)
            {
                var node = _queue.First;

                if (node is not null)
                {
                    item = node.Value;
                    _queue.Remove(item);
                    return true;
                }

                item = default;
                return false;
            }
        }
        public void Enqueue(T item)
        {
            lock (_sync)
            {
                _queue.AddLast(item); 
            }
        }
    }
}
