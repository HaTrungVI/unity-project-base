namespace ProjectBase.Common.Reward
{
    public struct RewardGrantedEvent
    {
        public RewardBundle Bundle;
    }

    public struct RewardGrantFailedEvent
    {
        public RewardBundle Bundle;
        public string Reason;
    }
}
