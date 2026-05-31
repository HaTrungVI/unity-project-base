using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBase.GameFlow
{
    public class UnloadLevelTask : IBootstrapTask
    {
        public string Name {get; }
        public float Weight {get; }
        public UnloadLevelTask(string name = "Unloading Level", float weight = 0.3f)
        {
            Name = name;
            Weight = weight;
        }
        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            // Implement your level unloading logic here
            // For example, you might want to unload specific scenes or assets related to the level

            // Simulate some work with a delay
            progress?.Report(0.5f);
            await UniTask.Delay(1000, cancellationToken: ct); // Simulate time taken to unload the level
            progress?.Report(1f);   
        }
    }
}
