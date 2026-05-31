using System;
using System.Collections.Generic;

namespace ProjectBase.Time.Data
{
    [Serializable]
    public class TimeData
    {
        public long lastSessionEndTicks;
        public long lastSyncTicks;
        public long driftTicks;
        public long lastDriftTicks;
        public List<TimerStateData> activeTimers = new();
        public List<ScheduleStateData> schedules = new();
        public long firstLaunchTicks;
        public long totalPlaytimeSeconds;
    }

    [Serializable]
    public class TimerStateData
    {
        public string tag;
        public float remainingSeconds;
        public long endTimeTicks;
        public bool autoRestart;
        public float originalDuration;
    }

    [Serializable]
    public class ScheduleStateData
    {
        public string scheduleId;
        public long lastResetTicks;
        public long lastClaimTicks;
        public int currentCycle;
        public bool isClaimed;
    }
}
