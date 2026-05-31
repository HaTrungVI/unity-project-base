using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.GameFlow
{
    public interface IBootstrapTask
    {
        string Name { get; }
        float Weight { get; }
        UniTask Execute(IProgress<float> progress, CancellationToken ct);
    }
}
