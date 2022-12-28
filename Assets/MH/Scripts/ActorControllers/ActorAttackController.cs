using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
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

        private IDisposable invokeScope;

        public ActorAttackController(Actor actor, ActorAttackData data)
        {
            this.actor = actor;
            this.data = data;
            this.motionData = this.data.motionDataList.ToDictionary(x => x.motionName);
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async UniTask InvokeAsync(AttackType attackType)
        {
            if (this.IsAttacking && this.currentMotionData == null)
            {
                return;
            }

            string motionName;
            
            if (this.currentAttackType == AttackType.None)
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

            this.invokeScope?.Dispose();
            this.canRotate = false;
            var animationBlendData = this.currentMotionData.animationBlendData;
            
            var scope = DisposableBag.CreateBuilder();
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canRotate)
                    {
                        return;
                    }
                    
                    this.actor.PostureController.Rotate(x.Rotation);
                })
                .AddTo(scope);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.canRotate = true;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.CloseRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.canRotate = false;
                })
                .AddTo(scope);

            this.invokeScope = scope.Build();

            await this.actor.AnimationController.PlayTask(animationBlendData);

            this.currentAttackType = AttackType.None;
            this.invokeScope?.Dispose();
            MessageBroker.GetPublisher<Actor, ActorEvents.EndAttack>()
                .Publish(this.actor, ActorEvents.EndAttack.Get());
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
                    return "WeakAttack.First";
                case AttackType.DodgeAttack:
                    return "DodgeAttack.First";
                case AttackType.None:
                default:
                    Assert.IsTrue(false, $"{attackType} is not supported.");
                    return "";
            }
        }
    }
}
