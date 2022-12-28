using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>のモデルを制御するクラス
    /// </summary>
    public sealed class ActorModelController : MonoBehaviour
    {
        [SerializeField]
        private BoneHolder boneHolder;

        public BoneHolder BoneHolder => this.boneHolder;
    }
}
