using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Common.Reward
{
    public interface IRewardGranter
    {
        UniTask<bool> GrantRewards(RewardBundle bundle, CancellationToken ct = default);
    }
}
