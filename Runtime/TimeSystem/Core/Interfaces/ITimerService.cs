using System;

namespace ProjectBase.Time.Core
{
    public interface ITimerService
    {
        TimerHandle CreateTimer(TimerConfig config);
        void CancelTimer(TimerHandle handle);
        void PauseTimer(TimerHandle handle);
        void ResumeTimer(TimerHandle handle);
        float GetRemainingTime(TimerHandle handle);
        float GetProgress(TimerHandle handle);
        bool IsTimerActive(TimerHandle handle);
        void CancelAllTimers();
        void CancelTimersByTag(string tag);
        TimerHandle FindTimerByTag(string tag);
        void SetTimerCallbacks(TimerHandle handle, Action<float> onTick, Action onComplete);
    }
}
