using System.Collections.Generic;
using UnityEngine;

namespace ProjectBase.Common.Patterns
{
    internal interface IObjectPoolInternal
    {
        void ReturnUntyped(Component instance);
        void Clear();
        int CountActive { get; }
        int CountAvailable { get; }
    }

    internal class ObjectPool<T> : IObjectPoolInternal where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _poolRoot;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _active = new();

        public int CountActive => _active.Count;
        public int CountAvailable => _available.Count;

        public ObjectPool(T prefab, Transform poolRoot)
        {
            _prefab = prefab;
            _poolRoot = poolRoot;
        }

        public void Warm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(_prefab, _poolRoot);
                instance.gameObject.SetActive(false);
                _available.Push(instance);
            }
        }

        public T Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance;
            if (_available.Count > 0)
            {
                instance = _available.Pop();
            }
            else
            {
                instance = Object.Instantiate(_prefab, _poolRoot);
            }

            var t = instance.transform;
            t.SetParent(parent != null ? parent : _poolRoot);
            t.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            _active.Add(instance);
            instance.OnSpawn();

            return instance;
        }

        public void Return(T instance)
        {
            if (!_active.Remove(instance)) return;

            instance.OnDespawn();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_poolRoot);
            _available.Push(instance);
        }

        public void ReturnUntyped(Component instance)
        {
            if (instance is T typed)
                Return(typed);
        }

        public void Clear()
        {
            foreach (var instance in _active)
            {
                if (instance != null)
                    Object.Destroy(instance.gameObject);
            }
            _active.Clear();

            while (_available.Count > 0)
            {
                var instance = _available.Pop();
                if (instance != null)
                    Object.Destroy(instance.gameObject);
            }
        }
    }
}
