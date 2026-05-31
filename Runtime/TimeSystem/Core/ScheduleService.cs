using System;
using System.Collections.Generic;
using ProjectBase.Common.Patterns;
using ProjectBase.Time.Data;
using ProjectBase.Time.Events;

namespace ProjectBase.Time.Core
{
    internal class ScheduleService
    {
        private readonly Func<DateTime> _getTrustedUtcNow;
        private readonly Dictionary<string, ScheduleRuntime> _schedules = new();
        private readonly Dictionary<string, ScheduleStateData> _pendingStates = new();

        public ScheduleService(Func<DateTime> getTrustedUtcNow)
        {
            _getTrustedUtcNow = getTrustedUtcNow;
        }

        public void Register(ScheduleDefinition definition)
        {
            if (_schedules.ContainsKey(definition.Id))
                return;

            var runtime = new ScheduleRuntime
            {
                Definition = definition,
                CurrentCycle = 0,
                IsClaimed = false
            };

            var now = _getTrustedUtcNow();
            runtime.NextResetUtc = CalculateNextReset(definition, now);
            runtime.LastResetUtc = CalculatePreviousReset(definition, now);

            if (_pendingStates.TryGetValue(definition.Id, out var savedState))
            {
                ApplyRestoredState(runtime, savedState, now);
                _pendingStates.Remove(definition.Id);
            }

            _schedules[definition.Id] = runtime;
        }

        public bool HasReset(string scheduleId)
        {
            if (!_schedules.TryGetValue(scheduleId, out var s)) return false;

            var now = _getTrustedUtcNow();
            if (now >= s.NextResetUtc)
            {
                s.LastResetUtc = s.NextResetUtc;
                s.NextResetUtc = CalculateNextReset(s.Definition, now);
                s.IsClaimed = false;
                s.CurrentCycle++;

                EventBus.Publish(new ScheduleResetEvent
                    { ScheduleId = scheduleId, NewCycle = s.CurrentCycle });
            }

            return !s.IsClaimed;
        }

        public TimeSpan GetTimeUntilReset(string scheduleId)
        {
            if (!_schedules.TryGetValue(scheduleId, out var s)) return TimeSpan.Zero;

            var remaining = s.NextResetUtc - _getTrustedUtcNow();
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public int GetCurrentCycle(string scheduleId)
        {
            return _schedules.TryGetValue(scheduleId, out var s) ? s.CurrentCycle : 0;
        }

        public void MarkClaimed(string scheduleId)
        {
            if (_schedules.TryGetValue(scheduleId, out var s))
                s.IsClaimed = true;
        }

        public List<ScheduleStateData> GetStates()
        {
            var result = new List<ScheduleStateData>();
            foreach (var kvp in _schedules)
            {
                var s = kvp.Value;
                result.Add(new ScheduleStateData
                {
                    scheduleId = s.Definition.Id,
                    lastResetTicks = s.LastResetUtc.Ticks,
                    lastClaimTicks = s.IsClaimed ? _getTrustedUtcNow().Ticks : 0,
                    currentCycle = s.CurrentCycle,
                    isClaimed = s.IsClaimed
                });
            }
            return result;
        }

        public void RestoreSchedules(List<ScheduleStateData> states)
        {
            if (states == null) return;

            var now = _getTrustedUtcNow();

            foreach (var state in states)
            {
                if (_schedules.TryGetValue(state.scheduleId, out var runtime))
                {
                    ApplyRestoredState(runtime, state, now);
                }
                else
                {
                    _pendingStates[state.scheduleId] = state;
                }
            }
        }

        private static void ApplyRestoredState(
            ScheduleRuntime runtime, ScheduleStateData state, DateTime now)
        {
            runtime.CurrentCycle = state.currentCycle;
            runtime.IsClaimed = state.isClaimed;
            runtime.LastResetUtc = new DateTime(state.lastResetTicks, DateTimeKind.Utc);
            runtime.NextResetUtc = CalculateNextReset(runtime.Definition, runtime.LastResetUtc);

            if (now >= runtime.NextResetUtc)
            {
                runtime.LastResetUtc = runtime.NextResetUtc;
                runtime.NextResetUtc = CalculateNextReset(runtime.Definition, now);
                runtime.IsClaimed = false;
                runtime.CurrentCycle++;
            }
        }

        private static DateTime CalculateNextReset(ScheduleDefinition def, DateTime from)
        {
            switch (def.Type)
            {
                case ScheduleType.Daily:
                    return CalculateNextDailyReset(def.ResetHourUtc, from);

                case ScheduleType.Weekly:
                    return CalculateNextWeeklyReset(
                        def.WeeklyResetDay, def.ResetHourUtc, from);

                case ScheduleType.Interval:
                    return from + def.CustomInterval;

                default:
                    return from.AddDays(1);
            }
        }

        private static DateTime CalculatePreviousReset(ScheduleDefinition def, DateTime from)
        {
            switch (def.Type)
            {
                case ScheduleType.Daily:
                {
                    var today = from.Date.AddHours(def.ResetHourUtc);
                    return from >= today ? today : today.AddDays(-1);
                }

                case ScheduleType.Weekly:
                {
                    var daysUntil = ((int)def.WeeklyResetDay - (int)from.DayOfWeek + 7) % 7;
                    var nextReset = from.Date.AddDays(daysUntil).AddHours(def.ResetHourUtc);
                    if (from >= nextReset && daysUntil == 0)
                        return nextReset;
                    return nextReset.AddDays(-7);
                }

                case ScheduleType.Interval:
                    return from;

                default:
                    return from;
            }
        }

        private static DateTime CalculateNextDailyReset(int resetHourUtc, DateTime from)
        {
            var todayReset = from.Date.AddHours(resetHourUtc);
            return from < todayReset ? todayReset : todayReset.AddDays(1);
        }

        private static DateTime CalculateNextWeeklyReset(
            DayOfWeek resetDay, int resetHourUtc, DateTime from)
        {
            var daysUntil = ((int)resetDay - (int)from.DayOfWeek + 7) % 7;
            var nextReset = from.Date.AddDays(daysUntil).AddHours(resetHourUtc);

            if (daysUntil == 0 && from >= nextReset)
                nextReset = nextReset.AddDays(7);

            return nextReset;
        }
    }

    internal class ScheduleRuntime
    {
        public ScheduleDefinition Definition;
        public DateTime NextResetUtc;
        public DateTime LastResetUtc;
        public bool IsClaimed;
        public int CurrentCycle;
    }
}
