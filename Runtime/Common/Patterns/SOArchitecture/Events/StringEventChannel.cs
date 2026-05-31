using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "StringEventChannel", menuName = "ProjectBase/Events/String Event Channel")]
    public class StringEventChannel : SOEventChannel<string> { }
}
