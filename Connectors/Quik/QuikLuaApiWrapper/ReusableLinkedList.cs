using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Quik
{
    [Serializable]
    public class ReusableLinkedList<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
    {
        // This ReusableLinkedList is a doubly-Linked circular list.
        internal ReusableLinkedListNode<T>? head;
        internal int count;
        internal int version;
        private SerializationInfo? _siInfo; //A temporary variable which we need during deserialization.
        private readonly Stack<ReusableLinkedListNode<T>> _pool;
        private const int DEFAULT_POOL_CAPACITY = 20;

        // names for serialization
        private const string VersionName = "Version"; // Do not rename (binary serialization)
        private const string CountName = "Count"; // Do not rename (binary serialization)
        private const string ValuesName = "Data"; // Do not rename (binary serialization)

        public ReusableLinkedList(int capacity)
        {
            _pool = new Stack<ReusableLinkedListNode<T>>();

            for (int i = 0; i < capacity; i++)
            {
                _pool.Push(new ReusableLinkedListNode<T>(this, default));
            }
        }
        public ReusableLinkedList() : this(DEFAULT_POOL_CAPACITY)
        {
        }

        public ReusableLinkedList(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (T item in collection)
            {
                AddLast(item);
            }
        }

        protected ReusableLinkedList(SerializationInfo info, StreamingContext context)
        {
            _siInfo = info;
        }

        public int Count
        {
            get { return count; }
        }

        public ReusableLinkedListNode<T>? First
        {
            get { return head; }
        }

        public ReusableLinkedListNode<T>? Last
        {
            get { return head?.prev; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T value)
        {
            AddLast(value);
        }

        private ReusableLinkedListNode<T> GetNode(T value)
        {
            if (_pool.TryPop(out ReusableLinkedListNode<T> item))
            {
                item.Value = value;
            }
            return new ReusableLinkedListNode<T>(this, value);
        }

        public ReusableLinkedListNode<T> AddAfter(ReusableLinkedListNode<T> node, T value)
        {
            ValidateNode(node);
            var result = GetNode(value);
            InternalInsertNodeBefore(node.next!, result);
            return result;
        }

        public void AddAfter(ReusableLinkedListNode<T> node, ReusableLinkedListNode<T> newNode)
        {
            ValidateNode(node);
            ValidateNewNode(newNode);
            InternalInsertNodeBefore(node.next!, newNode);
            newNode.list = this;
        }

        public ReusableLinkedListNode<T> AddBefore(ReusableLinkedListNode<T> node, T value)
        {
            ValidateNode(node);
            ReusableLinkedListNode<T> result = GetNode(value);
            InternalInsertNodeBefore(node, result);
            if (node == head)
            {
                head = result;
            }
            return result;
        }

        public void AddBefore(ReusableLinkedListNode<T> node, ReusableLinkedListNode<T> newNode)
        {
            ValidateNode(node);
            ValidateNewNode(newNode);
            InternalInsertNodeBefore(node, newNode);
            newNode.list = this;
            if (node == head)
            {
                head = newNode;
            }
        }

        public ReusableLinkedListNode<T> AddFirst(T value)
        {
            ReusableLinkedListNode<T> result = GetNode(value);
            if (head == null)
            {
                InternalInsertNodeToEmptyList(result);
            }
            else
            {
                InternalInsertNodeBefore(head, result);
                head = result;
            }
            return result;
        }

        public void AddFirst(ReusableLinkedListNode<T> node)
        {
            ValidateNewNode(node);

            if (head == null)
            {
                InternalInsertNodeToEmptyList(node);
            }
            else
            {
                InternalInsertNodeBefore(head, node);
                head = node;
            }
            node.list = this;
        }

        public ReusableLinkedListNode<T> AddLast(T value)
        {
            ReusableLinkedListNode<T> result = GetNode(value);
            if (head == null)
            {
                InternalInsertNodeToEmptyList(result);
            }
            else
            {
                InternalInsertNodeBefore(head, result);
            }
            return result;
        }

        public void AddLast(ReusableLinkedListNode<T> node)
        {
            ValidateNewNode(node);

            if (head == null)
            {
                InternalInsertNodeToEmptyList(node);
            }
            else
            {
                InternalInsertNodeBefore(head, node);
            }
            node.list = this;
        }

        public void Clear()
        {
            ReusableLinkedListNode<T>? current = head;
            while (current != null)
            {
                ReusableLinkedListNode<T> temp = current;
                current = current.Next;
                temp.Invalidate();
                _pool.Push(temp);
            }

            head = null;
            count = 0;
            version++;
        }

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        public void CopyTo(T[] array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Trying to set negative index");
            }

            if (index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index exceeds collection size {array.Length}");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Not enough space in array");
            }

            ReusableLinkedListNode<T>? node = head;
            if (node != null)
            {
                do
                {
                    array[index++] = node!.item;
                    node = node.next;
                } while (node != head);
            }
        }

        public ReusableLinkedListNode<T>? Find(T value)
        {
            ReusableLinkedListNode<T>? node = head;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null)
            {
                if (value != null)
                {
                    do
                    {
                        if (c.Equals(node!.item, value))
                        {
                            return node;
                        }
                        node = node.next;
                    } while (node != head);
                }
                else
                {
                    do
                    {
                        if (node!.item == null)
                        {
                            return node;
                        }
                        node = node.next;
                    } while (node != head);
                }
            }
            return null;
        }

        public ReusableLinkedListNode<T>? FindLast(T value)
        {
            if (head == null) return null;

            ReusableLinkedListNode<T>? last = head.prev;
            ReusableLinkedListNode<T>? node = last;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null)
            {
                if (value != null)
                {
                    do
                    {
                        if (c.Equals(node!.item, value))
                        {
                            return node;
                        }

                        node = node.prev;
                    } while (node != last);
                }
                else
                {
                    do
                    {
                        if (node!.item == null)
                        {
                            return node;
                        }
                        node = node.prev;
                    } while (node != last);
                }
            }
            return null;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T value)
        {
            ReusableLinkedListNode<T>? node = Find(value);
            if (node != null)
            {
                InternalRemoveNode(node);
                return true;
            }
            return false;
        }

        public void Remove(ReusableLinkedListNode<T> node)
        {
            ValidateNode(node);
            InternalRemoveNode(node);
        }

        public void RemoveFirst()
        {
            if (head == null) { throw new InvalidOperationException("Collection is empty"); }
            InternalRemoveNode(head);
        }

        public void RemoveLast()
        {
            if (head == null) { throw new InvalidOperationException("Collection is empty"); }
            InternalRemoveNode(head.prev!);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);

            // Customized serialization for ReusableLinkedList.
            // We need to do this because it will be too expensive to Serialize each node.
            // This will give us the flexiblility to change internal implementation freely in future.

            info.AddValue(VersionName, version);
            info.AddValue(CountName, count); // this is the length of the bucket array.

            if (count != 0)
            {
                T[] array = new T[count];
                CopyTo(array, 0);
                info.AddValue(ValuesName, array, typeof(T[]));
            }
        }

        public virtual void OnDeserialization(object? sender)
        {
            if (_siInfo == null)
            {
                return; //Somebody had a dependency on this ReusableLinkedList and fixed us up before the ObjectManager got to it.
            }

            int realVersion = _siInfo.GetInt32(VersionName);
            int count = _siInfo.GetInt32(CountName);

            if (count != 0)
            {
                T[]? array = (T[]?)_siInfo.GetValue(ValuesName, typeof(T[]));

                if (array == null)
                {
                    throw new SerializationException("No values provided");
                }
                for (int i = 0; i < array.Length; i++)
                {
                    AddLast(array[i]);
                }
            }
            else
            {
                head = null;
            }

            version = realVersion;
            _siInfo = null;
        }

        private void InternalInsertNodeBefore(ReusableLinkedListNode<T> node, ReusableLinkedListNode<T> newNode)
        {
            newNode.next = node;
            newNode.prev = node.prev;
            node.prev!.next = newNode;
            node.prev = newNode;
            version++;
            count++;
        }

        private void InternalInsertNodeToEmptyList(ReusableLinkedListNode<T> newNode)
        {
            Debug.Assert(head == null && count == 0, "ReusableLinkedList must be empty when this method is called!");
            newNode.next = newNode;
            newNode.prev = newNode;
            head = newNode;
            version++;
            count++;
        }

        internal void InternalRemoveNode(ReusableLinkedListNode<T> node)
        {
            Debug.Assert(node.list == this, "Deleting the node from another list!");
            Debug.Assert(head != null, "This method shouldn't be called on empty list!");
            if (node.next == node)
            {
                Debug.Assert(count == 1 && head == node, "this should only be true for a list with only one node");
                head = null;
            }
            else
            {
                node.next!.prev = node.prev;
                node.prev!.next = node.next;
                if (head == node)
                {
                    head = node.next;
                }
            }
            node.Invalidate();
            _pool.Push(node);
            count--;
            version++;
        }

        internal static void ValidateNewNode(ReusableLinkedListNode<T> node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (node.list != null)
            {
                throw new InvalidOperationException("Node is not attached");
            }
        }

        internal void ValidateNode(ReusableLinkedListNode<T> node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (node.list != this)
            {
                throw new InvalidOperationException("Node is not from this collection");
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot => this;

        void ICollection.CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Rank != 1)
            {
                throw new ArgumentException("Cannot copy to multidimensional array", nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Array is not zero-based", nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Trying to set negative index");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Not enough space in array");
            }

            T[]? tArray = array as T[];
            if (tArray != null)
            {
                CopyTo(tArray, index);
            }
            else
            {
                // No need to use reflection to verify that the types are compatible because it isn't 100% correct and we can rely
                // on the runtime validation during the cast that happens below (i.e. we will get an ArrayTypeMismatchException).
                object?[]? objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException("Array type not supported", nameof(array));
                }
                ReusableLinkedListNode<T>? node = head;
                try
                {
                    if (node != null)
                    {
                        do
                        {
                            objects[index++] = node!.item;
                            node = node.next;
                        } while (node != head);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("Array type not supported", nameof(array));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback
        {
            private readonly ReusableLinkedList<T> _list;
            private ReusableLinkedListNode<T>? _node;
            private readonly int _version;
            private T? _current;
            private int _index;

            internal Enumerator(ReusableLinkedList<T> list)
            {
                _list = list;
                _version = list.version;
                _node = list.head;
                _current = default;
                _index = 0;
            }

            public T Current => _current!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _list.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return Current;
                }
            }

            public bool MoveNext()
            {
                if (_version != _list.version)
                {
                    throw new InvalidOperationException();
                }

                if (_node == null)
                {
                    _index = _list.Count + 1;
                    return false;
                }

                ++_index;
                _current = _node.item;
                _node = _node.next;
                if (_node == _list.head)
                {
                    _node = null;
                }
                return true;
            }

            void IEnumerator.Reset()
            {
                if (_version != _list.version)
                {
                    throw new InvalidOperationException();
                }

                _current = default;
                _node = _list.head;
                _index = 0;
            }

            public void Dispose()
            {
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new PlatformNotSupportedException();
            }

            void IDeserializationCallback.OnDeserialization(object? sender)
            {
                throw new PlatformNotSupportedException();
            }
        }
    }

    // Note following class is not serializable since we customized the serialization of ReusableLinkedList.
    public sealed class ReusableLinkedListNode<T>
    {
        internal ReusableLinkedList<T>? list;
        internal ReusableLinkedListNode<T>? next;
        internal ReusableLinkedListNode<T>? prev;
        internal T item;

        public ReusableLinkedListNode(T value)
        {
            item = value;
        }

        internal ReusableLinkedListNode(ReusableLinkedList<T> list, T value)
        {
            this.list = list;
            item = value;
        }

        public ReusableLinkedList<T>? List
        {
            get { return list; }
        }

        public ReusableLinkedListNode<T>? Next
        {
            get { return next == null || next == list!.head ? null : next; }
        }

        public ReusableLinkedListNode<T>? Previous
        {
            get { return prev == null || this == list!.head ? null : prev; }
        }

        public T Value
        {
            get { return item; }
            set { item = value; }
        }

        /// <summary>Gets a reference to the value held by the node.</summary>
        public ref T ValueRef => ref item;

        internal void Invalidate()
        {
            next = null;
            prev = null;
        }
    }
}
