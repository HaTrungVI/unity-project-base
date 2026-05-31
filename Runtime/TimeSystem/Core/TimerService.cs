using System;
using System.Collections.Generic;
using ProjectBase.Common.Patterns;
using ProjectBase.Time.Data;
using ProjectBase.Time.Events;
using UnityEngine;

namespace ProjectBase.Time.Core
{
    internal class TimerService : ITimerService
    {
        private readonly List<GameTimer> _timers = new();
        private readonly Dictionary<int, GameTimer> _timerLookup = new();
        private readonly Func<DateTime> _getTrustedUtcNow;
        private int _nextId = 1;
        private bool _isTicking;

        public TimerService(Func<DateTime> getTrustedUtcNow)
        {
            _getTrustedUtcNow = getTrustedUtcNow;
        }

        public TimerHandle CreateTimer(TimerConfig config)
        {
            var timer = new GameTimer
            {
                Id = _nextId++,
                Tag = config.Tag ?? string.Empty,
                Duration = config.Duration,
                Remaining = config.Duration,
                Type = config.Type,
                PersistOffline = config.PersistOffline,
                AutoRestart = config.AutoRestart,
                OnTick = config.OnTick,
                OnComplete = config.OnComplete
            };

            if (config.PersistOffline)
                timer.EndTimeTicks = (_getTrustedUtcNow() + TimeSpan.FromSeconds(config.Duration)).Ticks;

            _timers.Add(timer);
            _timerLookup[timer.Id] = timer;
            return new TimerHandle(timer.Id);
        }

        public void CancelTimer(TimerHandle handle)
        {
            if (!_timerLookup.TryGetValue(handle.Id, out var timer)) return;

            var tag = timer.Tag;
            RemoveTimer(timer);
            EventBus.Publish(new TimerCancelledEvent { Tag = tag, Handle = handle });
        }

        public void PauseTimer(TimerHandle handle)
        {
            if (_timerLookup.TryGetValue(handle.Id, out var timer))
                timer.IsPaused = true;
        }

        public void ResumeTimer(TimerHandle handle)
        {
            if (_timerLookup.TryGetValue(handle.Id, out var timer))
                timer.IsPaused = false;
        }

        public float GetRemainingTime(TimerHandle handle)
        {
            if (_timerLookup.TryGetValue(handle.Id, out var timer))
                return Mathf.Max(0f, timer.Remaining);
            return 0f;
        }

        public float GetProgress(TimerHandle handle)
        {
            if (!_timerLookup.TryGetValue(handle.Id, out var timer) || timer.Duration <= 0f)
                return 1f;
            return Mathf.Clamp01(1f - timer.Remaining / timer.Duration);
        }

        public bool IsTimerActive(TimerHandle handle)
        {
            return _timerLookup.ContainsKey(handle.Id);
        }

        public void CancelAllTimers()
        {
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                var t = _timers[i];
                EventBus.Publish(new TimerCancelledEvent
                    { Tag = t.Tag, Handle = new TimerHandle(t.Id) });
            }
            _timers.Clear();
            _timerLookup.Clear();
        }

        public void CancelTimersByTag(string tag)
        {
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                var t = _timers[i];
                if (t.Tag == tag)
                {
                    SwapRemoveAt(i);
                    _timerLookup.Remove(t.Id);
                    EventBus.Publish(new TimerCancelledEvent
                        { Tag = t.Tag, Handle = new TimerHandle(t.Id) });
                }
            }
        }

        public void Tick(float deltaTime, float unscaledDeltaTime)
        {
            _isTicking = true;
            var removeCount = 0;

            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                var t = _timers[i];
                if (t.IsPaused) continue;

                var dt = t.Type == TimerType.Realtime ? unscaledDeltaTime : deltaTime;
                t.Remaining -= dt;

                t.OnTick?.Invoke(Mathf.Max(0f, t.Remaining));

                if (t.Remaining <= 0f)
                {
                    t.OnComplete?.Invoke();
                    EventBus.Publish(new TimerCompletedEvent
                        { Tag = t.Tag, Handle = new TimerHandle(t.Id) });

                    if (t.AutoRestart)
                    {
                        t.Remaining += t.Duration;
                        if (t.PersistOffline)
                            t.EndTimeTicks = (_getTrustedUtcNow()
                                + TimeSpan.FromSeconds(Mathf.Max(0f, t.Remaining))).Ticks;
                    }
                    else
                    {
                        _timerLookup.Remove(t.Id);
                        SwapRemoveAt(i);
                        removeCount++;
                    }
                }
            }

            _isTicking = false;
        }

        public void RestoreTimers(List<TimerStateData> states, DateTime trustedNow)
        {
            if (states == null) return;

            foreach (var state in states)
            {
                var endTime = new DateTime(state.endTimeTicks, DateTimeKind.Utc);
                var remaining = (float)(endTime - trustedNow).TotalSeconds;

                if (state.autoRestart)
                {
                    if (remaining <= 0f && state.originalDuration > 0f)
                    {
                        var elapsed = -remaining;
                        var completedCycles = (int)(elapsed / state.originalDuration);
                        remaining = state.originalDuration
                            - (elapsed - completedCycles * state.originalDuration);
                    }

                    var safeRemaining = Mathf.Max(0f, remaining);
                    var timer = new GameTimer
                    {
                        Id = _nextId++,
                        Tag = state.tag,
                        Duration = state.originalDuration,
                        Remaining = safeRemaining,
                        Type = TimerType.Realtime,
                        PersistOffline = true,
                        AutoRestart = true,
                        EndTimeTicks = (trustedNow
                            + TimeSpan.FromSeconds(safeRemaining)).Ticks
                    };
                    _timers.Add(timer);
                    _timerLookup[timer.Id] = timer;
                }
                else if (remaining > 0f)
                {
                    var timer = new GameTimer
                    {
                        Id = _nextId++,
                        Tag = state.tag,
                        Duration = state.originalDuration,
                        Remaining = remaining,
                        Type = TimerType.Realtime,
                        PersistOffline = true,
                        AutoRestart = false,
                        EndTimeTicks = (trustedNow
                            + TimeSpan.FromSeconds(remaining)).Ticks
                    };
                    _timers.Add(timer);
                    _timerLookup[timer.Id] = timer;
                }
            }
        }

        public List<TimerStateData> GetPersistableStates()
        {
            var result = new List<TimerStateData>();
            foreach (var t in _timers)
            {
                if (!t.PersistOffline) continue;
                result.Add(new TimerStateData
                {
                    tag = t.Tag,
                    remainingSeconds = t.Remaining,
                    endTimeTicks = t.EndTimeTicks,
                    autoRestart = t.AutoRestart,
                    originalDuration = t.Duration
                });
            }
            return result;
        }

        public TimerHandle FindTimerByTag(string tag)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Tag == tag)
                    return new TimerHandle(_timers[i].Id);
            }
            return TimerHandle.Invalid;
        }

        public void SetTimerCallbacks(TimerHandle handle, Action<float> onTick, Action onComplete)
        {
            if (!_timerLookup.TryGetValue(handle.Id, out var timer)) return;
            timer.OnTick = onTick;
            timer.OnComplete = onComplete;
        }

        private void RemoveTimer(GameTimer timer)
        {
            var index = _timers.IndexOf(timer);
            if (index >= 0)
                SwapRemoveAt(index);
            _timerLookup.Remove(timer.Id);
        }

        private void SwapRemoveAt(int index)
        {
            var lastIndex = _timers.Count - 1;
            if (index < lastIndex)
                _timers[index] = _timers[lastIndex];
            _timers.RemoveAt(lastIndex);
        }
    }
}
