using System;

namespace ProjectBase.Time.Core
{
    internal class GameTimer
    {
        public int Id;
        public string Tag;
        public float Duration;
        public float Remaining;
        public TimerType Type;
        public bool PersistOffline;
        public bool AutoRestart;
        public bool IsPaused;
        public Action<float> OnTick;
        public Action OnComplete;
        public long EndTimeTicks;
    }
}
