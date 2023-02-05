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
        /// ひるんだ時に相手の方向へ向き直すか
        /// </summary>
        public bool onFlinchRotationOppose;

        /// <summary>
        /// ガード成功とみなす角度
        /// </summary>
        public float guardSuccessAngle;

        /// <summary>
        /// スタミナ回復量（秒間）
        /// </summary>
        public float recoveryStamina;

        /// <summary>
        /// 回避に必要なスタミナの量
        /// </summary>
        public float dodgeStaminaAmount;

        /// <summary>
        /// ヒットポイント回復量（秒間）
        /// </summary>
        public float recoveryHitPoint;

        /// <summary>
        /// ヒットポイント回復量の消耗量（秒間）
        /// </summary>
        public float recoveryHitPointConsumePower;

        /// <summary>
        /// ヒットポイント回復量の最小値
        /// </summary>
        public float recoveryHitPointMin;

        /// <summary>
        /// <see cref="Actor"/>にアタッチするプレハブリスト
        /// </summary>
        /// <remarks>
        /// <see cref="IActorController"/>がアタッチされているプレハブを想定しています
        /// </remarks>
        public List<GameObject> extensionPrefabs;
    }
}
