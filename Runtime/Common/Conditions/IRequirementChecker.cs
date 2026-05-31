using System.Collections.Generic;

namespace ProjectBase.Common.Conditions
{
    public interface IRequirementChecker
    {
        bool Check(RequirementEntry requirement);
        bool CheckAll(IList<RequirementEntry> requirements);
    }

    public interface IRequirementProvider
    {
        RequirementType HandledType { get; }
        int GetValue(string targetId);
    }
}
