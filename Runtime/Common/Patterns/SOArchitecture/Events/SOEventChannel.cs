using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "EventChannel", menuName = "ProjectBase/Events/Event Channel")]
    public class SOEventChannel : ScriptableObject
    {
        [NonSerialized] private readonly List<Action> _listeners = new();

#if UNITY_EDITOR
        [NonSerialized] private readonly List<string> _subscriberNames = new();
        public IReadOnlyList<string> SubscriberNames => _subscriberNames;
        public int ListenerCount => _listeners.Count;
#endif

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke();
        }

        public void Subscribe(Action listener)
        {
            if (_listeners.Contains(listener)) return;
            _listeners.Add(listener);
#if UNITY_EDITOR
            _subscriberNames.Add($"{listener.Target?.GetType().Name}.{listener.Method.Name}");
#endif
        }

        public void Unsubscribe(Action listener)
        {
            var index = _listeners.IndexOf(listener);
            if (index < 0) return;
            _listeners.RemoveAt(index);
#if UNITY_EDITOR
            if (index < _subscriberNames.Count)
                _subscriberNames.RemoveAt(index);
#endif
        }
    }

    public abstract class SOEventChannel<T> : ScriptableObject
    {
        [NonSerialized] private readonly List<Action<T>> _listeners = new();

#if UNITY_EDITOR
        [NonSerialized] private readonly List<string> _subscriberNames = new();
        public IReadOnlyList<string> SubscriberNames => _subscriberNames;
        public int ListenerCount => _listeners.Count;
#endif

        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke(value);
        }

        public void Subscribe(Action<T> listener)
        {
            if (_listeners.Contains(listener)) return;
            _listeners.Add(listener);
#if UNITY_EDITOR
            _subscriberNames.Add($"{listener.Target?.GetType().Name}.{listener.Method.Name}");
#endif
        }

        public void Unsubscribe(Action<T> listener)
        {
            var index = _listeners.IndexOf(listener);
            if (index < 0) return;
            _listeners.RemoveAt(index);
#if UNITY_EDITOR
            if (index < _subscriberNames.Count)
                _subscriberNames.RemoveAt(index);
#endif
        }
    }
}
