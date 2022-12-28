using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 骨情報を保持するクラス
    /// </summary>
    public sealed class BoneHolder : MonoBehaviour
    {
        /// <summary>
        /// 右手
        /// </summary>
        [SerializeField]
        private Transform rightHand;

        public Transform RightHand => this.rightHand;
    }
}
