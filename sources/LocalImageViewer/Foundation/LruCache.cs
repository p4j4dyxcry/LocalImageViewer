using System.Collections.Generic;
using System.Threading;
namespace LocalImageViewer.Foundation
{
    public class LruCache<TKey, TValue>
    {
        private record LruCacheItem<TKey, TValue>(TKey Key, TValue Value);
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<LruCacheItem<TKey, TValue>>> _cacheMap = new();
        private readonly LinkedList<LruCacheItem<TKey, TValue>> _lruList = new();

        private SlimLocker _cacheLock = new ();

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

            LruCacheItem<TKey, TValue> cacheItem = new LruCacheItem<TKey, TValue>(key, value);
            LinkedListNode<LruCacheItem<TKey, TValue>> node = new LinkedListNode<LruCacheItem<TKey, TValue>>(cacheItem);
            _lruList.AddLast(node);
            _cacheMap[key] = node;
        }

        private void RemoveFirstInternal()
        {
            // Remove from LRUPriority
            LinkedListNode<LruCacheItem<TKey, TValue>> node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node!.Value.Key);
        }
    }
}
