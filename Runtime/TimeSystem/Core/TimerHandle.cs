using System;

namespace ProjectBase.Time.Core
{
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        public readonly int Id;

        public TimerHandle(int id) => Id = id;

        public bool IsValid => Id > 0;

        public bool Equals(TimerHandle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);
        public override int GetHashCode() => Id;

        public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);
        public static bool operator !=(TimerHandle left, TimerHandle right) => !left.Equals(right);

        public static readonly TimerHandle Invalid = new(0);
    }

    public enum TimerType
    {
        Realtime,
        GameTime
    }

    public class TimerConfig
    {
        public string Tag;
        public float Duration;
        public TimerType Type;
        public bool PersistOffline;
        public bool AutoRestart;
        public Action<float> OnTick;
        public Action OnComplete;
    }

    public class ScheduleDefinition
    {
        public string Id;
        public ScheduleType Type;
        public int ResetHourUtc;
        public DayOfWeek WeeklyResetDay;
        public TimeSpan CustomInterval;
    }

    public enum ScheduleType
    {
        Daily,
        Weekly,
        Interval
    }
}
