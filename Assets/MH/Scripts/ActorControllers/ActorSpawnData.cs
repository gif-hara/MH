using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>を生成するために必要なデータ
    /// </summary>
    [Serializable]
    public sealed class ActorSpawnData
    {
        public ActorStatus actorStatus;

        public ActorAttackData attackData;

        public ActorAnimationData animationData;

        /// <summary>
        /// 敵と接触した際に離れる距離
        /// </summary>
        public float leaveDistance;

        /// <summary>
        /// ひるんだ時に相手の方向へ向き直すか
        /// </summary>
        public bool onFlinchRotationOppose;

        /// <summary>
        /// <see cref="Actor"/>にアタッチするプレハブリスト
        /// </summary>
        /// <remarks>
        /// <see cref="IActorController"/>がアタッチされているプレハブを想定しています
        /// </remarks>
        public List<GameObject> extensionPrefabs;

        public IActorAI actorAI;
    }
}
