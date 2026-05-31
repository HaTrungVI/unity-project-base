using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "ProjectBase/Variables/Int")]
    public class IntVariable : SOVariable<int>
    {
        public void Add(int amount) => Value += amount;
        public void Subtract(int amount) => Value -= amount;
    }
}
