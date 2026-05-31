using System;
using System.Collections.Generic;

namespace ProjectBase.Common.Patterns
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, object> _listeners = new();
        private static readonly Dictionary<Type, object> _pendingAdds = new();
        private static readonly Dictionary<Type, object> _pendingRemoves = new();
        private static int _publishDepth;

        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            if (_publishDepth > 0)
            {
                GetOrCreatePendingList<T>(_pendingAdds).Add(listener);
                return;
            }
            GetOrCreateList<T>().Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            if (_publishDepth > 0)
            {
                GetOrCreatePendingList<T>(_pendingRemoves).Add(listener);
                return;
            }

            var type = typeof(T);
            if (_listeners.TryGetValue(type, out var obj))
                ((List<Action<T>>)obj).Remove(listener);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_listeners.TryGetValue(type, out var obj))
                return;

            var list = (List<Action<T>>)obj;
            if (list.Count == 0) return;

            _publishDepth++;
            try
            {
                var count = list.Count;
                for (int i = 0; i < count; i++)
                    list[i](eventData);
            }
            finally
            {
                _publishDepth--;
                if (_publishDepth == 0)
                    ApplyPendingChanges<T>();
            }
        }

        public static void Clear()
        {
            _listeners.Clear();
            _pendingAdds.Clear();
            _pendingRemoves.Clear();
        }

        public static void Clear<T>() where T : struct
        {
            _listeners.Remove(typeof(T));
            _pendingAdds.Remove(typeof(T));
            _pendingRemoves.Remove(typeof(T));
        }

        private static List<Action<T>> GetOrCreateList<T>() where T : struct
        {
            var type = typeof(T);
            if (!_listeners.TryGetValue(type, out var obj))
            {
                obj = new List<Action<T>>();
                _listeners[type] = obj;
            }
            return (List<Action<T>>)obj;
        }

        private static List<Action<T>> GetOrCreatePendingList<T>(Dictionary<Type, object> dict)
            where T : struct
        {
            var type = typeof(T);
            if (!dict.TryGetValue(type, out var obj))
            {
                obj = new List<Action<T>>();
                dict[type] = obj;
            }
            return (List<Action<T>>)obj;
        }

        private static void ApplyPendingChanges<T>() where T : struct
        {
            var type = typeof(T);

            if (_pendingRemoves.TryGetValue(type, out var removesObj))
            {
                var removes = (List<Action<T>>)removesObj;
                if (removes.Count > 0)
                {
                    var list = GetOrCreateList<T>();
                    foreach (var r in removes) list.Remove(r);
                    removes.Clear();
                }
            }

            if (_pendingAdds.TryGetValue(type, out var addsObj))
            {
                var adds = (List<Action<T>>)addsObj;
                if (adds.Count > 0)
                {
                    var list = GetOrCreateList<T>();
                    list.AddRange(adds);
                    adds.Clear();
                }
            }
        }
    }
}
