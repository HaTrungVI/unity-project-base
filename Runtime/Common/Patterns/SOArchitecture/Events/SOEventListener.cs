using UnityEngine;
using UnityEngine.Events;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    public class SOEventListener : MonoBehaviour
    {
        [SerializeField] private SOEventChannel _channel;
        [SerializeField] private UnityEvent _response;

        private void OnEnable()
        {
            if (_channel != null)
                _channel.Subscribe(OnRaised);
        }

        private void OnDisable()
        {
            if (_channel != null)
                _channel.Unsubscribe(OnRaised);
        }

        private void OnRaised()
        {
            _response?.Invoke();
        }
    }

    public abstract class SOEventListener<T> : MonoBehaviour
    {
        [SerializeField] private SOEventChannel<T> _channel;
        [SerializeField] private UnityEvent<T> _response;

        private void OnEnable()
        {
            if (_channel != null)
                _channel.Subscribe(OnRaised);
        }

        private void OnDisable()
        {
            if (_channel != null)
                _channel.Unsubscribe(OnRaised);
        }

        private void OnRaised(T value)
        {
            _response?.Invoke(value);
        }
    }

    public class IntEventListener : SOEventListener<int> { }
    public class FloatEventListener : SOEventListener<float> { }
    public class BoolEventListener : SOEventListener<bool> { }
    public class StringEventListener : SOEventListener<string> { }
}
