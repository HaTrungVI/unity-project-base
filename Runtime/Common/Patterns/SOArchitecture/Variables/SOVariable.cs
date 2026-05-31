using System;
using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    public abstract class SOVariable<T> : ScriptableObject
    {
        [SerializeField] private T _initialValue;

        [NonSerialized] private T _runtimeValue;
        [NonSerialized] private bool _isInitialized;
        [NonSerialized] private Action<T> _onValueChanged;

        public T Value
        {
            get => _isInitialized ? _runtimeValue : _initialValue;
            set
            {
                _runtimeValue = value;
                _isInitialized = true;
                _onValueChanged?.Invoke(value);
            }
        }

        public T InitialValue => _initialValue;

        public event Action<T> OnValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public void SetWithoutNotify(T value)
        {
            _runtimeValue = value;
            _isInitialized = true;
        }

        public void Reset()
        {
            _runtimeValue = _initialValue;
            _isInitialized = false;
        }

        private void OnEnable()
        {
            _runtimeValue = _initialValue;
            _isInitialized = false;
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }
}
