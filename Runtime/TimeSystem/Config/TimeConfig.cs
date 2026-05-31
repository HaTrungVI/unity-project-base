using UnityEngine;

namespace ProjectBase.Time.Config
{
    [CreateAssetMenu(fileName = "TimeConfig", menuName = "ProjectBase/Time/Time Config")]
    public class TimeConfig : ScriptableObject
    {
        [Header("Time Sync")]
        [SerializeField] private string _primaryTimeUrl = "https://worldtimeapi.org/api/ip";
        [SerializeField] private string _fallbackTimeUrl = "https://www.google.com";
        [SerializeField] private float _syncTimeoutSeconds = 5f;
        [SerializeField] private float _maxDriftDeltaSeconds = 60f;

        [Header("Offline")]
        [SerializeField] private float _maxOfflineHours = 24f;

        [Header("Auto Sync")]
        [SerializeField] private float _autoSyncIntervalMinutes = 30f;

        public string PrimaryTimeUrl => _primaryTimeUrl;
        public string FallbackTimeUrl => _fallbackTimeUrl;
        public float SyncTimeoutSeconds => _syncTimeoutSeconds;
        public float MaxDriftDeltaSeconds => _maxDriftDeltaSeconds;
        public float MaxOfflineHours => _maxOfflineHours;
        public float AutoSyncIntervalMinutes => _autoSyncIntervalMinutes;
    }
}
