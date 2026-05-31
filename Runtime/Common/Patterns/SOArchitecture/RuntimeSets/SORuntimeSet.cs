using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    public abstract class SORuntimeSet<T> : ScriptableObject
    {
        [NonSerialized] private readonly List<T> _items = new();

        public IReadOnlyList<T> Items => _items;
        public int Count => _items.Count;

        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;

        public void Add(T item)
        {
            if (_items.Contains(item)) return;
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void Remove(T item)
        {
            if (!_items.Remove(item)) return;
            OnItemRemoved?.Invoke(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        private void OnEnable()
        {
            _items.Clear();
        }

        private void OnDisable()
        {
            _items.Clear();
        }
    }

    [CreateAssetMenu(fileName = "GameObjectRuntimeSet",
        menuName = "ProjectBase/Runtime Sets/GameObject")]
    public class GameObjectRuntimeSet : SORuntimeSet<GameObject> { }
}
