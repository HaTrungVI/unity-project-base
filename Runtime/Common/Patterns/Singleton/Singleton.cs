using System;

namespace ProjectBase.Common.Patterns
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> _instance = new(() => new T());

        public static T Instance => _instance.Value;

        protected Singleton() { }
    }
}
