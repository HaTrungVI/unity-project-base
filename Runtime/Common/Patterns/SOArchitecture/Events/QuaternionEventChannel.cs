using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "QuaternionEventChannel", menuName = "ProjectBase/Events/Quaternion Event Channel")]
    public class QuaternionEventChannel : SOEventChannel<Quaternion> { }
}
