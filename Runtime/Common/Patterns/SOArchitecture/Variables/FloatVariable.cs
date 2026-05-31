using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "ProjectBase/Variables/Float")]
    public class FloatVariable : SOVariable<float>
    {
        public void Add(float amount) => Value += amount;
    }
}
