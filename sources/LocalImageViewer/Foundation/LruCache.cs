using System.Collections.Generic;

namespace LocalImageViewer.Foundation
{
    public class LruCache<TKey, TValue>
    {
        private record LruCacheItem(TKey Key, TValue Value);
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<LruCacheItem>> _cacheMap = new();
        private readonly LinkedList<LruCacheItem> _lruList = new();

        private readonly SlimLocker _cacheLock = new ();

        public LruCache(int capacity)
        {
            _capacity = capacity;
        }

        public TValue Get(TKey key)
        {
            using var _ = _cacheLock.ReadLock();
            if (!_cacheMap.TryGetValue(key, out var node))
            {
                return default;
            }
            TValue value = node.Value.Value;
            _lruList.Remove(node);
            _lruList.AddLast(node);
            return value;
        }

        public bool TryGet(TKey key,out TValue result)
        {
            using var _ = _cacheLock.ReadLock();
            if (_cacheMap.TryGetValue(key, out var node))
            {
                result = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return true;
            }
            result = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            using var _ = _cacheLock.WriteLock();
            AddInternal(key,value);
        }

        public void AddRange(IEnumerable<(TKey key, TValue val)> array)
        {
            using var _ = _cacheLock.WriteLock();
            foreach (var keyValue in array)
            {
                AddInternal(keyValue.key,keyValue.val);
            }
        }

        private void AddInternal(TKey key, TValue value)
        {
            if (_cacheMap.TryGetValue(key, out var existingNode))
            {
                _lruList.Remove(existingNode);
            }
            else if (_cacheMap.Count >= _capacity)
            {
                RemoveFirstInternal();
            }

            LruCacheItem cacheItem = new (key, value);
            LinkedListNode<LruCacheItem> node = new LinkedListNode<LruCacheItem>(cacheItem);
            _lruList.AddLast(node);
            _cacheMap[key] = node;
        }

        private void RemoveFirstInternal()
        {
            // Remove from LRUPriority
            LinkedListNode<LruCacheItem> node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node!.Value.Key);
        }
    }
}
