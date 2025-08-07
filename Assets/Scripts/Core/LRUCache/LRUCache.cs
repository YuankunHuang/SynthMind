using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class LRUCache<TKey, TValue>
    {
        private class CacheItem
        {
            public TKey Key;
            public TValue Value;

            public CacheItem(TKey k, TValue v)
            {
                Key = k;
                Value = v;
            }
        }

        private int _capacity;
        private Dictionary<TKey, LinkedListNode<CacheItem>> _cacheMap;
        private LinkedList<CacheItem> _lruList;

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _cacheMap = new();
            _lruList = new();
        }

        // try get
        public bool TryGet(TKey key, out TValue value)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        // add
        public void Add(TKey key, TValue value)
        {
            if (_cacheMap.TryGetValue(key, out var existingNode))
            {
                _lruList.Remove(existingNode);
            }
            else if (_cacheMap.Count >= _capacity)
            {
                var lastNode = _lruList.Last;
                if (lastNode != null)
                {
                    _lruList.Remove(lastNode);
                    _cacheMap.Remove(lastNode.Value.Key);
                }
            }

            var newNode = new LinkedListNode<CacheItem>(new CacheItem(key, value));
            _lruList.AddFirst(newNode);
            _cacheMap[key] = newNode;
        }

        // contains
        public bool Contains(TKey key) => _cacheMap.ContainsKey(key);

        // clear
        public void Clear()
        {
            _lruList.Clear();
            _cacheMap.Clear();
        }
    }
}