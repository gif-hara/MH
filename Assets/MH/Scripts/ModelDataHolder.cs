using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// モデルデータを保持するクラス
    /// </summary>
    public sealed class ModelDataHolder : MonoBehaviour
    {
        /// <summary>
        /// 右手
        /// </summary>
        [SerializeField]
        private Transform rightHand;

        private List<MeshRenderer> meshRenderers;

        public Transform RightHand => this.rightHand;

        public IReadOnlyList<MeshRenderer> MeshRenderers
        {
            get
            {
                this.meshRenderers ??= new List<MeshRenderer>(this.GetComponentsInChildren<MeshRenderer>());
                return this.meshRenderers;
            }
        }
    }
}
