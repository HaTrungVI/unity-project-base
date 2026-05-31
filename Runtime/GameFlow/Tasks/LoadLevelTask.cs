using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.GameFlow
{
    public class LoadLevelTask : IBootstrapTask
    {
        public string Name {get; }
        public float Weight {get; }
        public LoadLevelTask(string name = "Loading Level", float weight = 0.3f)
        {
            Name = name;
            Weight = weight;
        }
        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            // Implement your level loading logic here
            // For example, you might want to load specific scenes or assets related to the level

            // Simulate some work with a delay
            progress?.Report(0.5f);
            await UniTask.Delay(1000, cancellationToken: ct); // Simulate time taken to load the level
            progress?.Report(1f);   
        }
    }
}
