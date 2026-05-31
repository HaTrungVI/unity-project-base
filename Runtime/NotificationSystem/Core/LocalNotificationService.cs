using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBase.Notification
{
    public class LocalNotificationService : INotificationService
    {
        private readonly Dictionary<string, int> _scheduledIds = new();
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            InitializeAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            InitializeIOS();
#endif
            _isInitialized = true;
        }

        public void Schedule(string id, string title, string body, DateTime fireTimeUtc)
        {
            var delay = fireTimeUtc - DateTime.UtcNow;
            if (delay.TotalSeconds <= 0) return;
            Schedule(id, title, body, delay);
        }

        public void Schedule(string id, string title, string body, TimeSpan delay)
        {
            if (delay.TotalSeconds <= 0) return;
            Cancel(id);

#if UNITY_ANDROID && !UNITY_EDITOR
            ScheduleAndroid(id, title, body, delay);
#elif UNITY_IOS && !UNITY_EDITOR
            ScheduleIOS(id, title, body, delay);
#else
            Debug.Log($"[Notification] Scheduled '{id}': {title} in {delay.TotalMinutes:F0}m (editor only)");
#endif
        }

        public void Cancel(string id)
        {
            if (!_scheduledIds.ContainsKey(id)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            CancelAndroid(id);
#elif UNITY_IOS && !UNITY_EDITOR
            CancelIOS(id);
#endif
            _scheduledIds.Remove(id);
        }

        public void CancelAll()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CancelAllAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            CancelAllIOS();
#endif
            _scheduledIds.Clear();
        }

        // --- Platform implementations ---
        // Requires Unity Mobile Notifications package (com.unity.mobile.notifications)
        // These are stubs — uncomment and implement when package is imported

#if UNITY_ANDROID && !UNITY_EDITOR
        private void InitializeAndroid()
        {
            // var channel = new Unity.Notifications.Android.AndroidNotificationChannel
            // {
            //     Id = "game_channel",
            //     Name = "Game Notifications",
            //     Importance = Unity.Notifications.Android.Importance.Default,
            //     Description = "Game notifications"
            // };
            // Unity.Notifications.Android.AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        private void ScheduleAndroid(string id, string title, string body, TimeSpan delay)
        {
            // var notification = new Unity.Notifications.Android.AndroidNotification
            // {
            //     Title = title,
            //     Text = body,
            //     FireTime = DateTime.Now + delay,
            //     SmallIcon = "icon_small",
            //     LargeIcon = "icon_large"
            // };
            // var nid = Unity.Notifications.Android.AndroidNotificationCenter
            //     .SendNotification(notification, "game_channel");
            // _scheduledIds[id] = nid;
        }

        private void CancelAndroid(string id)
        {
            // if (_scheduledIds.TryGetValue(id, out var nid))
            //     Unity.Notifications.Android.AndroidNotificationCenter.CancelNotification(nid);
        }

        private void CancelAllAndroid()
        {
            // Unity.Notifications.Android.AndroidNotificationCenter.CancelAllNotifications();
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private void InitializeIOS()
        {
            // Unity.Notifications.iOS.iOSNotificationCenter
            //     .RequestAuthorization(AuthorizationOption.Alert | AuthorizationOption.Sound);
        }

        private void ScheduleIOS(string id, string title, string body, TimeSpan delay)
        {
            // var notification = new Unity.Notifications.iOS.iOSNotification
            // {
            //     Identifier = id,
            //     Title = title,
            //     Body = body,
            //     ShowInForeground = false,
            //     Trigger = new Unity.Notifications.iOS.iOSNotificationTimeIntervalTrigger
            //     {
            //         TimeInterval = delay,
            //         Repeats = false
            //     }
            // };
            // Unity.Notifications.iOS.iOSNotificationCenter.ScheduleNotification(notification);
            // _scheduledIds[id] = 0;
        }

        private void CancelIOS(string id)
        {
            // Unity.Notifications.iOS.iOSNotificationCenter.RemoveScheduledNotification(id);
        }

        private void CancelAllIOS()
        {
            // Unity.Notifications.iOS.iOSNotificationCenter.RemoveAllScheduledNotifications();
        }
#endif
    }
}
