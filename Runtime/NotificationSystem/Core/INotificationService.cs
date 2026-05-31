using System;

namespace ProjectBase.Notification
{
    public interface INotificationService
    {
        void Initialize();
        void Schedule(string id, string title, string body, DateTime fireTimeUtc);
        void Schedule(string id, string title, string body, TimeSpan delay);
        void Cancel(string id);
        void CancelAll();
    }
}
