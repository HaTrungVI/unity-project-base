using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "GameObjectEventChannel", menuName = "ProjectBase/Events/GameObject Event Channel")]
    public class GameObjectEventChannel : SOEventChannel<GameObject> { }
}
