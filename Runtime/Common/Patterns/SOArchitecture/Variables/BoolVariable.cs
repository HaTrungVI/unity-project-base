using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "ProjectBase/Variables/Bool")]
    public class BoolVariable : SOVariable<bool>
    {
        public void Toggle() => Value = !Value;
    }
}
