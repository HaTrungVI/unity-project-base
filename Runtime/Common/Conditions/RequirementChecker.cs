using System.Collections.Generic;
using UnityEngine;

namespace ProjectBase.Common.Conditions
{
    public class RequirementChecker : IRequirementChecker
    {
        private readonly Dictionary<RequirementType, IRequirementProvider> _providers = new();

        public void RegisterProvider(IRequirementProvider provider)
        {
            _providers[provider.HandledType] = provider;
        }

        public void UnregisterProvider(RequirementType type)
        {
            _providers.Remove(type);
        }

        public bool Check(RequirementEntry requirement)
        {
            if (!_providers.TryGetValue(requirement.type, out var provider))
            {
                Debug.LogWarning($"[RequirementChecker] No provider for {requirement.type}");
                return false;
            }

            int currentValue = provider.GetValue(requirement.targetId);
            return requirement.Evaluate(currentValue);
        }

        public bool CheckAll(IList<RequirementEntry> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return true;

            for (int i = 0; i < requirements.Count; i++)
            {
                if (!Check(requirements[i]))
                    return false;
            }
            return true;
        }
    }
}
