#define INVERSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public sealed class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private sealed class ValueCollection : ICollection<TValue>
        {
            private IndexedDictionary<TKey, TValue> owner;

            public ValueCollection(IndexedDictionary<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            #region Члены ICollection<TValue>

            public void Add(TValue item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TValue item)
            {
                for (var i = owner.items.Length; i-- > 0; )
                {
                    if (object.Equals(owner.items[i].Value, item) && owner.ContainsKey(owner.items[i].Key))
                        return true;
                }
                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TValue item)
            {
                throw new InvalidOperationException();
            }

            #endregion

            #region Члены IEnumerable<TValue>

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Value;
            }

            #endregion

            #region Члены IEnumerable

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Value;
            }

            #endregion
        }

        private sealed class KeyCollection : ICollection<TKey>
        {
            private IndexedDictionary<TKey, TValue> owner;

            public KeyCollection(IndexedDictionary<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            #region Члены ICollection<TKey>

            public void Add(TKey item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TKey item)
            {
                return owner.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TKey item)
            {
                throw new InvalidOperationException();
            }

            #endregion

            #region Члены IEnumerable<TKey>

            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Key;
            }

            #endregion

            #region Члены IEnumerable

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Key;
            }

            #endregion
        }

        private sealed class _ListNode
        {
            public TKey key;
            public _ListNode next;
            public int index;
        }

        [Serializable]
        private sealed class _Node
        {
            public int hash;
            public _Node _0;
            public _Node _1;
            public TKey key;
            public int index;
            /// <summary>
            /// For linked list implementation
            /// </summary>
            public _ListNode list;

            public override string ToString()
            {
                return "key = " + key + ", index = " + index;
            }
        }

        private KeyValuePair<TKey, TValue>[] items;
        private List<int> emptyIndexes;
        private _Node root;
        private int size;
        private IComparer<TKey> comparer;
        private static readonly KeyValuePair<TKey, TValue>[] emptyArray = new KeyValuePair<TKey, TValue>[0];

        public IndexedDictionary()
        {
            if (!typeof(IComparable).IsAssignableFrom(typeof(TKey)))
                throw new ArgumentException(typeof(TKey) + " do not immplement IComparable interface");
            comparer = Comparer<TKey>.Default;
            items = emptyArray;
        }

        private int findIndex(TKey key)
        {
            if (root == null)
                return -1;
            var hash = key.GetHashCode();
            var node = root;
#if INVERSE
            for (var i = sizeof(int) * 8; node != null && i-- > 0; )
#else
            for (var i = 0; node != null && i < sizeof(int) * 8; i++)
#endif
            {
                if (node.hash == hash)
                    break;
                if ((hash & (1 << i)) == 0)
                    node = node._0;
                else
                    node = node._1;
            }
            if (node == null)
                return -1;
            if (comparer.Compare(node.key, key) != 0)
            {
                var listItem = node.list;
                while (listItem != null && comparer.Compare(listItem.key, key) != 0)
                    listItem = listItem.next;
                if (listItem == null)
                    return -1;
                return listItem.index;
            }
            else
                return node.index;
        }

        private void insert(TKey key, TValue value, bool @throw)
        {
            _Node prewNode = null;
            _ListNode prew = null;
            _ListNode listItem = null;
            var node = root;
            var hash = key.GetHashCode();
            if (root == null)
            {
                placeKVPair(ref root, hash, key, value);
                return;
            }
#if INVERSE
            var i = sizeof(int) * 8;
            for (; node != null && i-- >= 0; )
#else
            var i = 0;
            for (; node != null && i <= sizeof(int) * 8; i++)
#endif
            {
                if (node.index == -1) // zombie node
                {
                    node.index = popEmptyIndex();
                    node.hash = hash;
                    node.key = key;
                    items[node.index] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
                if (node.hash == hash)
                {
                    if (comparer.Compare(node.key, key) != 0)
                    {
                        listItem = node.list;
                        while (listItem != null && comparer.Compare(listItem.key, key) != 0)
                        {
                            prew = listItem;
                            listItem = listItem.next;
                        }
                        if (listItem == null)
                        {
                            if (prew == null)
                                placeKVPair(ref node.list, hash, key, value);
                            else
                                placeKVPair(ref prew.next, hash, key, value);
                        }
                        else
                        {
                            if (@throw)
                                throw new InvalidOperationException();
                            items[listItem.index] = new KeyValuePair<TKey, TValue>(key, value);
                        }
                    }
                    else
                    {
                        if (@throw)
                            throw new InvalidOperationException();
                        items[node.index] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    return;
                }
#if INVERSE
                if (i == -1)
                {
                    i++;
#else
                if (i == sizeof(int) * 8)
                {
#endif
                    break;
                }
                prewNode = node;
                if ((hash & (1 << i)) == 0)
                    node = node._0;
                else
                    node = node._1;
            }
            // here node is null reference
#if DEBUG
            if (node != null)
                System.Diagnostics.Debugger.Break();
#endif
#if INVERSE
            if ((hash & (1 << i)) == 0)
#else
            if ((hash & (1 << --i)) == 0)
#endif
                placeKVPair(ref prewNode._0, hash, key, value);
            else
                placeKVPair(ref prewNode._1, hash, key, value);
        }

        private void placeKVPair(ref _Node dest, int hash, TKey key, TValue value)
        {
            dest = new _Node()
            {
                hash = hash,
                key = key,
                index = popEmptyIndex()
            };
            items[dest.index] = new KeyValuePair<TKey, TValue>(key, value);
        }

        private void placeKVPair(ref _ListNode dest, int hash, TKey key, TValue value)
        {
            dest = new _ListNode()
            {
                key = key,
                index = popEmptyIndex()
            };
            items[dest.index] = new KeyValuePair<TKey, TValue>(key, value);
        }

        private int popEmptyIndex()
        {
            if (emptyIndexes != null)
            {
                int res = emptyIndexes[emptyIndexes.Count - 1];
                if (emptyIndexes.Count == 1)
                    emptyIndexes = null;
                else
                    emptyIndexes.RemoveAt(emptyIndexes.Count - 1);
                size++;
                return res;
            }
            if (size == items.Length)
            {
                var newItems = new KeyValuePair<TKey, TValue>[Math.Max(2, items.Length * 2)];
                for (var i = 0; i < items.Length; i++)
                    newItems[i] = items[i];
                items = newItems;
            }
            size++;
            return size - 1;
        }

        #region Члены IDictionary<TKey,TValue>

        public void Add(TKey key, TValue value)
        {
            insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return findIndex(key) != -1;
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            if (root == null)
                return false;
            var hash = key.GetHashCode();
            var node = root;
#if INVERSE
            var i = sizeof(int) * 8;
            for (; node != null && i-- > 0; )
#else
            var i = 0;
            for (; node != null && i < sizeof(int) * 8; i++)
#endif
            {
                if (node.index != -1 && node.hash == hash)
                    break;
                if ((hash & (1 << i)) == 0)
                    node = node._0;
                else
                    node = node._1;
            }
            if (node == null)
                return false;
            if (comparer.Compare(node.key, key) != 0)
            {
                _ListNode prewList = null;
                var listItem = node.list;
                while (listItem != null && comparer.Compare(listItem.key, key) != 0)
                {
                    prewList = listItem;
                    listItem = listItem.next;
                }
                if (listItem == null)
                    return false;
                if (prewList == null)
                    node.list = null;
                else
                    prewList.next = listItem.next;
                pushEmptyIndex(listItem.index);
            }
            else
            {
                pushEmptyIndex(node.index);
                if (node.list != null)
                {
                    node.key = node.list.key;
                    node.list = node.list.next;
                    node.index = node.list.index;
                }
                else
                    node.index = -1; // make zombie
            }
            size--;
            return true;
        }

        private void pushEmptyIndex(int index)
        {
            items[index] = default(KeyValuePair<TKey, TValue>);
            (emptyIndexes ?? (emptyIndexes = new List<int>())).Add(index);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            var index = findIndex(key);
            if (index == -1)
                return false;
            value = items[index].Value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                var index = findIndex(key);
                if (index == -1)
                    throw new KeyNotFoundException();
                return items[index].Value;
            }
            set
            {
                insert(key, value, false);
            }
        }

        #endregion

        #region Члены ICollection<KeyValuePair<TKey,TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            size = 0;
            items = emptyArray;
            emptyIndexes = null;
            root = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value = default(TValue);
            return TryGetValue(item.Key, out value) && object.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return size; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<TKey,TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (root == null || size == 0)
                yield break;
            var stack = new _Node[32];
            int stackIndex = 0;
            var node = root;
            bool skip0 = false;
            for (; ; )
            {
                if (!skip0)
                {
#if INVERSE
                    while (node._1 != null)
                    {
                        stack[stackIndex++] = node;
                        node = node._1;
                    }
#else
                    while (node._0 != null)
                    {
                        stack[stackIndex++] = node;
                        node = node._0;
                    }
#endif
                }
                else
                    skip0 = false;
                if (node.index != -1)
                    yield return items[node.index];
                var list = node.list;
                while (list != null)
                {
                    yield return items[list.index];
                    list = list.next;
                }
#if INVERSE
                if (node._0 != null)
                    node = node._0;
#else
                if (node._1 != null)
                    node = node._1;
#endif
                else
                {
                    if (stackIndex == 0)
                        yield break;
                    node = stack[--stackIndex];
                    skip0 = true;
                }
            }
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
