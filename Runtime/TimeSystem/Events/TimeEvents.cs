using System;

namespace ProjectBase.Time.Events
{
    public struct TimeVerifiedEvent
    {
        public bool IsVerified;
    }

    public struct TimeTamperedEvent
    {
        public TimeSpan DriftDelta;
    }

    public struct TimerCompletedEvent
    {
        public string Tag;
        public Core.TimerHandle Handle;
    }

    public struct TimerCancelledEvent
    {
        public string Tag;
        public Core.TimerHandle Handle;
    }

    public struct ScheduleResetEvent
    {
        public string ScheduleId;
        public int NewCycle;
    }

    public struct OfflineTimeCalculatedEvent
    {
        public TimeSpan Duration;
        public bool IsCapped;
    }

    public struct SessionStartedEvent
    {
        public TimeSpan OfflineDuration;
    }
}
