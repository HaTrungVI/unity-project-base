using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectBase.Common.Patterns
{
    public interface IState
    {
        UniTask EnterAsync(CancellationToken ct);
        UniTask ExitAsync(CancellationToken ct);
        void Tick();
    }
}
