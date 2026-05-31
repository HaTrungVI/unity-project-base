using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Time.Config;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectBase.Time.Core
{
    internal class TrustedTimeProvider
    {
        private readonly TimeConfig _config;
        private TimeSpan _drift;
        private TimeSpan _lastDrift;
        private TimeSpan _driftDelta;
        private bool _isVerified;
        private bool _isTampered;
        private TimeSpan _offlineDuration;
        private bool _hasPreviousDrift;

        public DateTime TrustedUtcNow => DateTime.UtcNow + _drift;
        public bool IsVerified => _isVerified;
        public bool IsTampered => _isTampered;
        public TimeSpan OfflineDuration => _offlineDuration;
        public TimeSpan Drift => _drift;
        public TimeSpan DriftDelta => _driftDelta;

        public TrustedTimeProvider(TimeConfig config)
        {
            _config = config;
        }

        public void RestoreDrift(long driftTicks, long lastDriftTicks)
        {
            _drift = new TimeSpan(driftTicks);
            _lastDrift = new TimeSpan(lastDriftTicks);
            _hasPreviousDrift = driftTicks != 0;
        }

        public async UniTask SyncAsync(CancellationToken ct)
        {
            DateTime? serverTime = null;

            serverTime = await FetchTimeFromWorldTimeApi(ct);

            if (serverTime == null)
                serverTime = await FetchTimeFromHttpHeader(_config.FallbackTimeUrl, ct);

            if (serverTime.HasValue)
            {
                _lastDrift = _drift;
                _drift = serverTime.Value - DateTime.UtcNow;
                _isVerified = true;

                _driftDelta = _drift - _lastDrift;
                if (_hasPreviousDrift
                    && Math.Abs(_driftDelta.TotalSeconds) > _config.MaxDriftDeltaSeconds)
                {
                    _isTampered = true;
                    Debug.LogWarning(
                        $"[TimeSystem] Time tampering detected. Drift delta: {_driftDelta.TotalSeconds:F1}s");
                }

                _hasPreviousDrift = true;
            }
            else
            {
                _isVerified = false;
                Debug.LogWarning("[TimeSystem] Failed to sync server time. Using cached drift.");
            }
        }

        public void CalculateOfflineDuration(long lastSessionEndTicks)
        {
            if (lastSessionEndTicks == 0)
            {
                _offlineDuration = TimeSpan.Zero;
                return;
            }

            var lastEnd = new DateTime(lastSessionEndTicks, DateTimeKind.Utc);
            _offlineDuration = TrustedUtcNow - lastEnd;

            if (_offlineDuration < TimeSpan.Zero)
            {
                _isTampered = true;
                _offlineDuration = TimeSpan.Zero;
                Debug.LogWarning("[TimeSystem] Negative offline duration detected (clock set back).");
            }
        }

        private async UniTask<DateTime?> FetchTimeFromWorldTimeApi(CancellationToken ct)
        {
            try
            {
                var startTime = UnityEngine.Time.realtimeSinceStartup;

                using var request = UnityWebRequest.Get(_config.PrimaryTimeUrl);
                request.timeout = (int)_config.SyncTimeoutSeconds;

                await request.SendWebRequest().ToUniTask(cancellationToken: ct);

                var rtt = UnityEngine.Time.realtimeSinceStartup - startTime;

                if (request.result != UnityWebRequest.Result.Success)
                    return null;

                var json = request.downloadHandler.text;
                var response = JsonUtility.FromJson<WorldTimeApiResponse>(json);

                if (response.unixtime <= 0) return null;

                var serverTime = DateTimeOffset.FromUnixTimeSeconds(response.unixtime).UtcDateTime;
                serverTime = serverTime.AddSeconds(rtt / 2f);

                return serverTime;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TimeSystem] WorldTimeApi fetch failed: {e.Message}");
                return null;
            }
        }

        private async UniTask<DateTime?> FetchTimeFromHttpHeader(string url, CancellationToken ct)
        {
            try
            {
                var startTime = UnityEngine.Time.realtimeSinceStartup;

                using var request = UnityWebRequest.Head(url);
                request.timeout = (int)_config.SyncTimeoutSeconds;

                await request.SendWebRequest().ToUniTask(cancellationToken: ct);

                var rtt = UnityEngine.Time.realtimeSinceStartup - startTime;

                if (request.result != UnityWebRequest.Result.Success)
                    return null;

                var dateHeader = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(dateHeader)) return null;

                if (!DateTimeOffset.TryParseExact(
                        dateHeader,
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out var parsedDate))
                    return null;

                var serverTime = parsedDate.UtcDateTime.AddSeconds(rtt / 2f);
                return serverTime;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TimeSystem] HTTP header time fetch failed: {e.Message}");
                return null;
            }
        }

        [Serializable]
        private struct WorldTimeApiResponse
        {
            public long unixtime;
        }
    }
}
