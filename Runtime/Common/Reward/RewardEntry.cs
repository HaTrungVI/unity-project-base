using System;
using System.Collections.Generic;

namespace ProjectBase.Common.Reward
{
    public enum RewardType
    {
        Currency,
        Item,
        Hero,
        Equipment
    }

    [Serializable]
    public class RewardEntry
    {
        public RewardType type;
        public string id;
        public int quantity = 1;

        public RewardEntry() { }

        public RewardEntry(RewardType type, string id, int quantity)
        {
            this.type = type;
            this.id = id;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class RewardBundle
    {
        public string description;
        public List<RewardEntry> entries = new();

        public RewardBundle() { }

        public RewardBundle(string description)
        {
            this.description = description;
        }

        public RewardBundle Add(RewardType type, string id, int quantity)
        {
            entries.Add(new RewardEntry(type, id, quantity));
            return this;
        }

        public bool IsEmpty => entries == null || entries.Count == 0;
    }
}
