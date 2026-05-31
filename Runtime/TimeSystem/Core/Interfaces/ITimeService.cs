using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Time.Core
{
    public interface ITimeService
    {
        DateTime TrustedUtcNow { get; }
        bool IsTimeVerified { get; }
        bool IsTimeTampered { get; }
        TimeSpan OfflineDuration { get; }
        TimeSpan SessionDuration { get; }
        long TotalPlaytimeSeconds { get; }

        UniTask SyncTimeAsync(CancellationToken ct);

        void RegisterSchedule(ScheduleDefinition definition);
        bool HasScheduleReset(string scheduleId);
        TimeSpan GetTimeUntilReset(string scheduleId);
        int GetCurrentCycle(string scheduleId);
        void MarkScheduleClaimed(string scheduleId);
    }
}
