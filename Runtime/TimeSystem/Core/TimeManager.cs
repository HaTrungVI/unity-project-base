using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using ProjectBase.Time.Config;
using ProjectBase.Time.Data;
using ProjectBase.Time.Events;
using UnityEngine;

namespace ProjectBase.Time.Core
{
    public class TimeManager : MonoBehaviour, ITimeService, ITimerService
    {
        [SerializeField] private TimeConfig _config;
        [SerializeField] private TimeModule _dataModule;
        [SerializeField] private TimeServiceRef _serviceRef;

        private TrustedTimeProvider _timeProvider;
        private TimerService _timerService;
        private ScheduleService _scheduleService;
        private CancellationTokenSource _cts;
        private bool _isInitialized;
        private float _sessionStartRealtimeSinceStartup;

        public DateTime TrustedUtcNow => _timeProvider.TrustedUtcNow;
        public bool IsTimeVerified => _timeProvider.IsVerified;
        public bool IsTimeTampered => _timeProvider.IsTampered;
        public TimeSpan OfflineDuration => _timeProvider.OfflineDuration;

        public TimeSpan SessionDuration => TimeSpan.FromSeconds(
            UnityEngine.Time.realtimeSinceStartup - _sessionStartRealtimeSinceStartup);

        public long TotalPlaytimeSeconds => _dataModule.TotalPlaytimeSeconds;
        public bool IsInitialized => _isInitialized;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _timeProvider = new TrustedTimeProvider(_config);
            _timerService = new TimerService(() => _timeProvider.TrustedUtcNow);
            _scheduleService = new ScheduleService(() => _timeProvider.TrustedUtcNow);
            _sessionStartRealtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;

            _timeProvider.RestoreDrift(_dataModule.DriftTicks, _dataModule.LastDriftTicks);

            await _timeProvider.SyncAsync(_cts.Token);

            _dataModule.UpdateDrift(_timeProvider.Drift, _timeProvider.TrustedUtcNow);

            _timeProvider.CalculateOfflineDuration(_dataModule.LastSessionEndTicks);

            var offlineDuration = _timeProvider.OfflineDuration;
            EventBus.Publish(new OfflineTimeCalculatedEvent
            {
                Duration = offlineDuration,
                IsCapped = offlineDuration.TotalHours > _config.MaxOfflineHours
            });

            _timerService.RestoreTimers(
                _dataModule.GetTimerStates(), _timeProvider.TrustedUtcNow);

            _scheduleService.RestoreSchedules(_dataModule.GetScheduleStates());

            if (_dataModule.FirstLaunchTicks == 0)
                _dataModule.SetFirstLaunch(_timeProvider.TrustedUtcNow);

            _serviceRef.Bind(this, _timerService);
            _isInitialized = true;

            EventBus.Publish(new SessionStartedEvent { OfflineDuration = offlineDuration });

            RunAutoSyncLoop(_cts.Token).Forget();
        }

        private void Update()
        {
            if (!_isInitialized) return;
            _timerService.Tick(UnityEngine.Time.deltaTime, UnityEngine.Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
            if (!_isInitialized) return;

            if (paused)
            {
                SaveSessionState();
            }
            else
            {
                ResyncOnResume().Forget();
            }
        }

        private void OnApplicationQuit()
        {
            if (_isInitialized)
                SaveSessionState();
        }

        private void OnDestroy()
        {
            if (_serviceRef != null)
                _serviceRef.Unbind();

            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTask SyncTimeAsync(CancellationToken ct)
        {
            await _timeProvider.SyncAsync(ct);
            _dataModule.UpdateDrift(_timeProvider.Drift, _timeProvider.TrustedUtcNow);

            EventBus.Publish(new TimeVerifiedEvent { IsVerified = _timeProvider.IsVerified });

            if (_timeProvider.IsTampered)
                EventBus.Publish(new TimeTamperedEvent { DriftDelta = _timeProvider.DriftDelta });
        }

        public void RegisterSchedule(ScheduleDefinition definition)
        {
            _scheduleService.Register(definition);
        }

        public bool HasScheduleReset(string scheduleId)
        {
            return _scheduleService.HasReset(scheduleId);
        }

        public TimeSpan GetTimeUntilReset(string scheduleId)
        {
            return _scheduleService.GetTimeUntilReset(scheduleId);
        }

        public int GetCurrentCycle(string scheduleId)
        {
            return _scheduleService.GetCurrentCycle(scheduleId);
        }

        public void MarkScheduleClaimed(string scheduleId)
        {
            _scheduleService.MarkClaimed(scheduleId);
        }

        public TimerHandle CreateTimer(TimerConfig config)
        {
            return _timerService.CreateTimer(config);
        }

        public void CancelTimer(TimerHandle handle)
        {
            _timerService.CancelTimer(handle);
        }

        public void PauseTimer(TimerHandle handle)
        {
            _timerService.PauseTimer(handle);
        }

        public void ResumeTimer(TimerHandle handle)
        {
            _timerService.ResumeTimer(handle);
        }

        public float GetRemainingTime(TimerHandle handle)
        {
            return _timerService.GetRemainingTime(handle);
        }

        public float GetProgress(TimerHandle handle)
        {
            return _timerService.GetProgress(handle);
        }

        public bool IsTimerActive(TimerHandle handle)
        {
            return _timerService.IsTimerActive(handle);
        }

        public void CancelAllTimers()
        {
            _timerService.CancelAllTimers();
        }

        public void CancelTimersByTag(string tag)
        {
            _timerService.CancelTimersByTag(tag);
        }

        public TimerHandle FindTimerByTag(string tag)
        {
            return _timerService.FindTimerByTag(tag);
        }

        public void SetTimerCallbacks(TimerHandle handle, Action<float> onTick, Action onComplete)
        {
            _timerService.SetTimerCallbacks(handle, onTick, onComplete);
        }

        private void SaveSessionState()
        {
            _dataModule.UpdateSessionEnd(_timeProvider.TrustedUtcNow);
            _dataModule.SaveTimerStates(_timerService.GetPersistableStates());
            _dataModule.SaveScheduleStates(_scheduleService.GetStates());

            var sessionSeconds = UnityEngine.Time.realtimeSinceStartup
                - _sessionStartRealtimeSinceStartup;
            _dataModule.AddPlaytime(sessionSeconds);

            _sessionStartRealtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        }

        private async UniTaskVoid ResyncOnResume()
        {
            try
            {
                await _timeProvider.SyncAsync(_cts.Token);
                _dataModule.UpdateDrift(_timeProvider.Drift, _timeProvider.TrustedUtcNow);

                EventBus.Publish(new TimeVerifiedEvent { IsVerified = _timeProvider.IsVerified });

                if (_timeProvider.IsTampered)
                    EventBus.Publish(new TimeTamperedEvent
                        { DriftDelta = _timeProvider.DriftDelta });
            }
            catch (OperationCanceledException) { }
        }

        private async UniTaskVoid RunAutoSyncLoop(CancellationToken ct)
        {
            var intervalMs = (int)(_config.AutoSyncIntervalMinutes * 60 * 1000);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(intervalMs, cancellationToken: ct);
                    await _timeProvider.SyncAsync(ct);
                    _dataModule.UpdateDrift(_timeProvider.Drift, _timeProvider.TrustedUtcNow);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
