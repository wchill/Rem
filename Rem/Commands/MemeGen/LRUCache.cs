using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rem.Commands.MemeGen
{
    public class LRUCache<K, V>
    {
        private readonly int _capacity;

        private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap =
            new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();

        private readonly LinkedList<LRUCacheItem<K, V>> _lruList = new LinkedList<LRUCacheItem<K, V>>();

        public LRUCache(int capacity)
        {
            _capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryGet(K key, out V value)
        {
            if (cacheMap.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return true;
            }

            value = default(V);
            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(K key, V val)
        {
            if (cacheMap.TryGetValue(key, out var existingNode))
            {
                _lruList.Remove(existingNode);
            }

            if (cacheMap.Count >= _capacity)
            {
                RemoveFirst();
            }

            var cacheItem = new LRUCacheItem<K, V>(key, val);
            var node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
            _lruList.AddLast(node);
            cacheMap.Add(key, node);
        }

        private void RemoveFirst()
        {
            // Remove from LRUPriority
            var node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            cacheMap.Remove(node.Value.Key);
        }
    }

    class LRUCacheItem<K, V>
    {
        public LRUCacheItem(K k, V v)
        {
            Key = k;
            Value = v;
        }

        public readonly K Key;
        public readonly V Value;
    }
}
