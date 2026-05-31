using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectBase.Common;
using UnityEngine;

namespace ProjectBase.GameFlow
{
    public abstract class BaseSplashController : MonoBehaviour
    {
        [SerializeField] private GameFlowConfig _config;

        private readonly List<IBootstrapTask> _tasks = new();
        private float _totalWeight;

        protected GameFlowConfig Config => _config;

        protected void AddTask(IBootstrapTask task)
        {
            _tasks.Add(task);
            _totalWeight += task.Weight;
        }

        protected abstract void SetupTasks();
        protected abstract void OnProgressUpdated(float normalizedProgress, string taskName);
        protected abstract void OnAllCompleted();

        private async UniTaskVoid Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            SetupTasks();

            if (_totalWeight <= 1f)
                _totalWeight = 1f;

            float completedWeight = 0f;
            var startTime = Time.realtimeSinceStartup;

            foreach (var task in _tasks)
            {
                var capturedWeight = completedWeight;
                var taskWeight = task.Weight;

                var taskProgress = new SyncProgress<float>(p =>
                {
                    var overall = (capturedWeight + p * taskWeight) / _totalWeight;
                    OnProgressUpdated(overall, task.Name);
                });

                await task.Execute(taskProgress, ct);
                completedWeight += task.Weight;
            }

            var elapsed = Time.realtimeSinceStartup - startTime;
            var remaining = _config.MinimumSplashTime - elapsed;
            if (remaining > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(remaining), cancellationToken: ct);

            OnProgressUpdated(1f, "Done");
            OnAllCompleted();
        }
    }
}
