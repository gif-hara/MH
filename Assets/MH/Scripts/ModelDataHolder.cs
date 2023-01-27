using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// モデルデータを保持するクラス
    /// </summary>
    public sealed class ModelDataHolder : MonoBehaviour
    {
        /// <summary>
        /// モデルのパーツタイプ
        /// </summary>
        public enum PartType
        {
            /// <summary>右手</summary>
            RightHand,
        }

        [SerializeField]
        private List<PartData> dataList;

        private List<MeshRenderer> meshRenderers;

        public IReadOnlyList<MeshRenderer> MeshRenderers
        {
            get
            {
                this.meshRenderers ??= new List<MeshRenderer>(this.GetComponentsInChildren<MeshRenderer>());
                return this.meshRenderers;
            }
        }

        public Transform GetPart(PartType type)
        {
            var result = this.dataList.Find(x => x.Type == type);
            Assert.IsNotNull(result, $"{type}に紐づくデータがありません");

            return result.Part;
        }

        [Serializable]
        public class PartData
        {
            [SerializeField]
            private PartType type;

            [SerializeField]
            private Transform part;

            public PartType Type => this.type;

            public Transform Part => this.part;
        }
    }
}
