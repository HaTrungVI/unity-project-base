using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProjectBase.Common.Patterns
{
    public class StateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();
        private IState _currentState;
        private bool _isTransitioning;

        public IState CurrentState => _currentState;
        public Type CurrentStateType { get; private set; }

        public void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        public async UniTask ChangeStateAsync<T>(CancellationToken ct) where T : IState
        {
            if (_isTransitioning) return;

            var targetType = typeof(T);
            if (!_states.TryGetValue(targetType, out var nextState)) return;

            _isTransitioning = true;
            try
            {
                if (_currentState != null)
                    await _currentState.ExitAsync(ct);

                _currentState = nextState;
                CurrentStateType = targetType;
                await _currentState.EnterAsync(ct);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void Tick()
        {
            if (!_isTransitioning)
                _currentState?.Tick();
        }

        public T GetState<T>() where T : IState
        {
            return _states.TryGetValue(typeof(T), out var state) ? (T)state : default;
        }
    }
}
