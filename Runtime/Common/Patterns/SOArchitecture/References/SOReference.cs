using System;
using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [Serializable]
    public class SOReference<T>
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private T _constantValue;
        [SerializeField] private SOVariable<T> _variable;

        public SOReference() { }

        public SOReference(T constantValue)
        {
            _useConstant = true;
            _constantValue = constantValue;
        }

        public T Value
        {
            get => _useConstant ? _constantValue : _variable.Value;
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else
                    _variable.Value = value;
            }
        }

        public bool UseConstant => _useConstant;
        public SOVariable<T> Variable => _variable;

        public static implicit operator T(SOReference<T> reference) => reference.Value;
    }
}
