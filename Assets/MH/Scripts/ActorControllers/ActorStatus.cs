using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public sealed class ActorStatus
    {
        public Define.ActorType actorType;

        public int hitPoint;

        public List<PartData> partDataList;

        public ActorStatus(ActorStatus other)
        {
            this.actorType = other.actorType;
            this.hitPoint = other.hitPoint;
            this.partDataList = new List<PartData>(other.partDataList);
        }

        [Serializable]
        public class PartData
        {
            /// <summary>
            /// 部位の名前
            /// </summary>
            [SerializeField]
            private string partName;

            /// <summary>
            /// 耐久値
            /// </summary>
            [SerializeField]
            private int endurance;

            /// <summary>
            /// ダメージを適用する割合
            /// </summary>
            /// <remarks>
            /// いわゆる肉質
            /// </remarks>
            [SerializeField][Range(0.0f, 1.0f)]
            private float damageRate;

            public string PartName => this.partName;

            public int Endurance => this.endurance;

            public float DamageRate => this.damageRate;
        }
    }
}
