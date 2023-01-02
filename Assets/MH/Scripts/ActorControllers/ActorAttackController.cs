using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorAttackController
    {
        public enum AttackType
        {
            None,
            WeakAttack,
            DodgeAttack,
            StrongAttack,
        }
        private Actor actor;

        private ActorAttackData data;

        private Dictionary<string, ActorAttackData.MotionData> motionData;

        private int currentAttackIndex;

        private bool canRotate;

        /// <summary>
        /// 攻撃中であるか
        /// </summary>
        private bool isAttacking = false;

        private AttackType currentAttackType;

        private ActorAttackData.MotionData currentMotionData;

        public ActorAttackController(Actor actor, ActorAttackData data)
        {
            this.actor = actor;
            this.data = data;
            this.motionData = this.data.motionDataList.ToDictionary(x => x.motionName);
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async void Invoke(AttackType attackType)
        {
            string motionName;
            
            // 攻撃していなかった場合はタイプから初回の攻撃を算出する
            if (this.currentAttackType == AttackType.None || this.currentAttackType != attackType)
            {
                this.currentAttackType = attackType;
                motionName = this.GetFirstMotionName(this.currentAttackType);
                this.currentMotionData = this.motionData[motionName];
            }
            else
            {
                if (this.currentMotionData.nextMotionName == "")
                {
                    motionName = "";
                }
                else
                {
                    this.currentMotionData = this.motionData[this.currentMotionData.nextMotionName];
                    motionName = this.currentMotionData.motionName;
                }
            }
            
            if (motionName == "")
            {
                return;
            }

            this.canRotate = false;
            var animationBlendData = this.currentMotionData.animationBlendData;

            try
            {
                await this.actor.AnimationController.PlayAsync(animationBlendData);
            
                this.Reset();
                MessageBroker.GetPublisher<Actor, ActorEvents.EndAttack>()
                    .Publish(this.actor, ActorEvents.EndAttack.Get());
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Reset()
        {
            this.currentAttackType = AttackType.None;
            this.currentMotionData = null;
        }

        /// <summary>
        /// 攻撃中であるか返す
        /// </summary>
        private bool IsAttacking => this.currentAttackType != AttackType.None;

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
                    Assert.IsTrue(false, $"{attackType} is not supported.");
                    return "";
            }
        }
    }
}
