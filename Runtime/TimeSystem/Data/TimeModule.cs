using System;
using System.Collections.Generic;
using ProjectBase.Data.Core;
using UnityEngine;

namespace ProjectBase.Time.Data
{
    [CreateAssetMenu(fileName = "TimeModule", menuName = "ProjectBase/Time/Time Module")]
    public class TimeModule : DataModule<TimeData>
    {
        public long LastSessionEndTicks => Data.lastSessionEndTicks;
        public long TotalPlaytimeSeconds => Data.totalPlaytimeSeconds;
        public long DriftTicks => Data.driftTicks;
        public long LastDriftTicks => Data.lastDriftTicks;
        public long FirstLaunchTicks => Data.firstLaunchTicks;

        public void UpdateSessionEnd(DateTime utcNow)
        {
            Data.lastSessionEndTicks = utcNow.Ticks;
            MarkDirty();
        }

        public void SaveTimerStates(List<TimerStateData> states)
        {
            Data.activeTimers = states;
            MarkDirty();
        }

        public List<TimerStateData> GetTimerStates()
        {
            return Data.activeTimers ?? new List<TimerStateData>();
        }

        public void SaveScheduleStates(List<ScheduleStateData> states)
        {
            Data.schedules = states;
            MarkDirty();
        }

        public List<ScheduleStateData> GetScheduleStates()
        {
            return Data.schedules ?? new List<ScheduleStateData>();
        }

        public void UpdateDrift(TimeSpan drift, DateTime syncTime)
        {
            Data.lastDriftTicks = Data.driftTicks;
            Data.driftTicks = drift.Ticks;
            Data.lastSyncTicks = syncTime.Ticks;
            MarkDirty();
        }

        public void AddPlaytime(float seconds)
        {
            Data.totalPlaytimeSeconds += (long)seconds;
            MarkDirty();
        }

        public void SetFirstLaunch(DateTime utcNow)
        {
            Data.firstLaunchTicks = utcNow.Ticks;
            MarkDirty();
        }
    }
}
