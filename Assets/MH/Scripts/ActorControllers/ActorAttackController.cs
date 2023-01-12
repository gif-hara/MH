using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の攻撃を制御するクラス
    /// </summary>
    public sealed class ActorAttackController
    {
        public enum AttackType
        {
            None,
            WeakAttack,
            DodgeAttack,
            StrongAttack
        }

        private readonly Actor actor;

        private readonly ActorAttackData data;

        private readonly Dictionary<string, ActorAttackData.MotionData> motionData;

        private int currentAttackIndex;

        private AttackType currentAttackType;

        private ActorAttackData.MotionData currentMotionData;

        public ActorAttackController(Actor actor, ActorAttackData data)
        {
            this.actor = actor;
            this.data = data;
            motionData = this.data.motionDataList.ToDictionary(x => x.motionName);
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async void Invoke(AttackType attackType)
        {
            string motionName;

            // 攻撃していなかった場合はタイプから初回の攻撃を算出する
            if (currentAttackType == AttackType.None || currentAttackType != attackType)
            {
                currentAttackType = attackType;
                motionName = GetFirstMotionName(currentAttackType);
                currentMotionData = motionData[motionName];
            }
            else
            {
                if (currentMotionData.nextMotionName == "")
                {
                    motionName = "";
                }
                else
                {
                    currentMotionData = motionData[currentMotionData.nextMotionName];
                    motionName = currentMotionData.motionName;
                }
            }

            if (motionName == "")
            {
                return;
            }

            var animationBlendData = currentMotionData.animationBlendData;

            try
            {
                await actor.AnimationController.PlayAsync(animationBlendData);

                Reset();
                MessageBroker.GetPublisher<Actor, ActorEvents.EndAttack>()
                    .Publish(actor, ActorEvents.EndAttack.Get());
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Reset()
        {
            currentAttackType = AttackType.None;
            currentMotionData = null;
        }

        /// <summary>
        /// 最初の攻撃のモーション名を返す
        /// </summary>
        private string GetFirstMotionName(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.WeakAttack:
                    return "WeakAttack.0";
                case AttackType.DodgeAttack:
                    return "DodgeAttack";
                case AttackType.StrongAttack:
                    return "StrongAttack.0";
                case AttackType.None:
                default:
                    Assert.IsTrue(
                        false,
                        $"{attackType} is not supported."
                        );
                    return "";
            }
        }
    }
}
