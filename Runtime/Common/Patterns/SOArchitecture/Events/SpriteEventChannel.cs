using UnityEngine;

namespace ProjectBase.Common.Patterns.SOArchitecture
{
    [CreateAssetMenu(fileName = "SpriteEventChannel", menuName = "ProjectBase/Events/Sprite Event Channel")]
    public class SpriteEventChannel : SOEventChannel<Sprite> { }
}
