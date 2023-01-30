using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の部位を表すコンポーネント
    /// </summary>
    public sealed class ActorPartComponent : MonoBehaviour
    {
        [SerializeField]
        private Define.PartType partType;

        public Define.PartType PartType => this.partType;
    }
}
