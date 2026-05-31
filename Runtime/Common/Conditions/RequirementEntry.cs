using System;

namespace ProjectBase.Common.Conditions
{
    public enum RequirementType
    {
        PlayerLevel,
        CurrencyAmount,
        ItemOwned,
        StageClear,
        HeroOwned,
        VIPLevel,
        AchievementComplete,
        QuestComplete,
        Custom
    }

    public enum ComparisonOp
    {
        GreaterOrEqual,
        LessOrEqual,
        Equal,
        GreaterThan,
        LessThan
    }

    [Serializable]
    public class RequirementEntry
    {
        public RequirementType type;
        public string targetId;
        public int targetValue;
        public ComparisonOp comparison = ComparisonOp.GreaterOrEqual;

        public RequirementEntry() { }

        public RequirementEntry(RequirementType type, string targetId, int targetValue,
            ComparisonOp comparison = ComparisonOp.GreaterOrEqual)
        {
            this.type = type;
            this.targetId = targetId;
            this.targetValue = targetValue;
            this.comparison = comparison;
        }

        public bool Evaluate(int currentValue)
        {
            return comparison switch
            {
                ComparisonOp.GreaterOrEqual => currentValue >= targetValue,
                ComparisonOp.LessOrEqual => currentValue <= targetValue,
                ComparisonOp.Equal => currentValue == targetValue,
                ComparisonOp.GreaterThan => currentValue > targetValue,
                ComparisonOp.LessThan => currentValue < targetValue,
                _ => false
            };
        }
    }
}
