using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBase.Time.Core
{
    [CreateAssetMenu(fileName = "TimeServiceRef", menuName = "ProjectBase/Time/Time Service Ref")]
    public class TimeServiceRef : ScriptableObject, ITimeService, ITimerService
    {
        [NonSerialized] private ITimeService _timeService;
        [NonSerialized] private ITimerService _timerService;

        public bool IsBound => _timeService != null;

        public DateTime TrustedUtcNow =>
            _timeService != null ? _timeService.TrustedUtcNow : DateTime.UtcNow;

        public bool IsTimeVerified =>
            _timeService != null && _timeService.IsTimeVerified;

        public bool IsTimeTampered =>
            _timeService != null && _timeService.IsTimeTampered;

        public TimeSpan OfflineDuration =>
            _timeService?.OfflineDuration ?? TimeSpan.Zero;

        public TimeSpan SessionDuration =>
            _timeService?.SessionDuration ?? TimeSpan.Zero;

        public long TotalPlaytimeSeconds =>
            _timeService?.TotalPlaytimeSeconds ?? 0;

        public UniTask SyncTimeAsync(CancellationToken ct)
        {
            EnsureBound();
            return _timeService.SyncTimeAsync(ct);
        }

        public void RegisterSchedule(ScheduleDefinition definition)
        {
            EnsureBound();
            _timeService.RegisterSchedule(definition);
        }

        public bool HasScheduleReset(string scheduleId)
        {
            EnsureBound();
            return _timeService.HasScheduleReset(scheduleId);
        }

        public TimeSpan GetTimeUntilReset(string scheduleId)
        {
            EnsureBound();
            return _timeService.GetTimeUntilReset(scheduleId);
        }

        public int GetCurrentCycle(string scheduleId)
        {
            EnsureBound();
            return _timeService.GetCurrentCycle(scheduleId);
        }

        public void MarkScheduleClaimed(string scheduleId)
        {
            EnsureBound();
            _timeService.MarkScheduleClaimed(scheduleId);
        }

        public TimerHandle CreateTimer(TimerConfig config)
        {
            EnsureBound();
            return _timerService.CreateTimer(config);
        }

        public void CancelTimer(TimerHandle handle)
        {
            EnsureBound();
            _timerService.CancelTimer(handle);
        }

        public void PauseTimer(TimerHandle handle)
        {
            EnsureBound();
            _timerService.PauseTimer(handle);
        }

        public void ResumeTimer(TimerHandle handle)
        {
            EnsureBound();
            _timerService.ResumeTimer(handle);
        }

        public float GetRemainingTime(TimerHandle handle)
        {
            EnsureBound();
            return _timerService.GetRemainingTime(handle);
        }

        public float GetProgress(TimerHandle handle)
        {
            EnsureBound();
            return _timerService.GetProgress(handle);
        }

        public bool IsTimerActive(TimerHandle handle)
        {
            if (_timerService == null) return false;
            return _timerService.IsTimerActive(handle);
        }

        public void CancelAllTimers()
        {
            EnsureBound();
            _timerService.CancelAllTimers();
        }

        public void CancelTimersByTag(string tag)
        {
            EnsureBound();
            _timerService.CancelTimersByTag(tag);
        }

        public TimerHandle FindTimerByTag(string tag)
        {
            EnsureBound();
            return _timerService.FindTimerByTag(tag);
        }

        public void SetTimerCallbacks(TimerHandle handle, Action<float> onTick, Action onComplete)
        {
            EnsureBound();
            _timerService.SetTimerCallbacks(handle, onTick, onComplete);
        }

        internal void Bind(ITimeService timeService, ITimerService timerService)
        {
            _timeService = timeService;
            _timerService = timerService;
        }

        internal void Unbind()
        {
            _timeService = null;
            _timerService = null;
        }

        private void EnsureBound()
        {
            if (_timeService == null || _timerService == null)
                throw new InvalidOperationException(
                    "TimeServiceRef is not bound. Ensure TimeManager is initialized before use.");
        }
    }
}
