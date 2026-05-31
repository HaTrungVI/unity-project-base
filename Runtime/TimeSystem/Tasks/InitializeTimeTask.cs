using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.GameFlow;
using ProjectBase.Time.Core;

namespace ProjectBase.Time.Tasks
{
    public class InitializeTimeTask : IBootstrapTask
    {
        private readonly TimeManager _timeManager;

        public string Name => "Initializing Time System";
        public float Weight { get; }

        public InitializeTimeTask(TimeManager timeManager, float weight = 0.15f)
        {
            _timeManager = timeManager;
            Weight = weight;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);
            await _timeManager.InitializeAsync(ct);
            progress?.Report(1f);
        }
    }
}
