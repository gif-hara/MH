using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
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
        /// <see cref="Actor"/>にアタッチするプレハブリスト
        /// </summary>
        /// <remarks>
        /// <see cref="IActorController"/>がアタッチされているプレハブを想定しています
        /// </remarks>
        public List<GameObject> extensionPrefabs;
    }
}
