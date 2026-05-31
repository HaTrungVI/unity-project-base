using System;
using System.Collections.Generic;

namespace ProjectBase.Common.Extensions
{
    public static class CollectionExtensions
    {
        private static readonly Random _rng = new();

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static T RandomElement<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Cannot get random element from empty list.");
            return list[_rng.Next(list.Count)];
        }

        public static T RandomElement<T>(this IList<T> list, Random rng)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Cannot get random element from empty list.");
            return list[rng.Next(list.Count)];
        }

        public static bool TryGet<T>(this IList<T> list, int index, out T value)
        {
            if (list != null && index >= 0 && index < list.Count)
            {
                value = list[index];
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryGetValue<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (dict is Dictionary<TKey, TValue> concrete)
                return concrete.TryGetValue(key, out value);

            if (dict.ContainsKey(key))
            {
                value = dict[key];
                return true;
            }
            value = default;
            return false;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
