using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の攻撃を制御するクラス
    /// </summary>
    public sealed class ActorAttackController : IActorController
    {
        public enum AttackType
        {
            None,
            WeakAttack,
            DodgeAttack,
            StrongAttack
        }

        private Actor actor;

        private int currentAttackIndex;

        private AttackType currentAttackType;

        private ActorAttackData.MotionData currentMotionData;

        private ActorAttackData data;

        private Dictionary<string, ActorAttackData.MotionData> motionData;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.actor = actor;
            this.data = spawnData.attackData;
            this.motionData = this.data.motionDataList.ToDictionary(x => x.motionName);
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public void Invoke(AttackType attackType)
        {
            string motionName;

            if (this.currentAttackType == AttackType.None || this.currentAttackType != attackType)
            {
                this.currentAttackType = attackType;
                motionName = GetFirstMotionName(this.currentAttackType);
            }
            else
            {
                if (this.currentMotionData.nextMotionName == "")
                {
                    motionName = "";
                }
                else
                {
                    motionName = this.motionData[this.currentMotionData.nextMotionName].motionName;
                }
            }

            if (motionName == "")
            {
                return;
            }

            Invoke(motionName);
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async void Invoke(string motionName)
        {
            Assert.AreNotEqual(motionName, "");
            this.currentMotionData = this.motionData[motionName];

            try
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginAttack>()
                    .Publish(actor, ActorEvents.BeginAttack.Get(motionName));

                await actor.AnimationController.PlayAsync(motionName);

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
