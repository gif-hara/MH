using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// アニメーションイベントと<see cref="Actor"/>を繋ぎこむクラス
    /// </summary>
    public sealed class ActorAnimationEventMediator : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        private Animator animator;
        
        private void Start()
        {
            this.animator = this.GetComponent<Animator>();
            Assert.IsNotNull(this.animator);
        }

        /// <summary>
        /// 回転のリクエストの受付を開始する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void AcceptRequestRotation()
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.AcceptRequestRotation>()
                .Publish(this.actor, ActorEvents.AcceptRequestRotation.Get());
        }

        /// <summary>
        /// 回転のリクエストの受付を終了する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void CloseRequestRotation()
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.CloseRequestRotation>()
                .Publish(this.actor, ActorEvents.CloseRequestRotation.Get());
        }

        /// <summary>
        /// 次の状態を選択可能な状態にする
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void AcceptNextState()
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.AcceptNextState>()
                .Publish(this.actor, ActorEvents.AcceptNextState.Get());
        }

        /// <summary>
        /// 攻撃判定を有効化する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void ValidationAttackCollider(string colliderName)
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.ValidationAttackCollider>()
                .Publish(this.actor, ActorEvents.ValidationAttackCollider.Get(colliderName));
        }

        /// <summary>
        /// 攻撃判定を無効化する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void InvalidationAttackCollider(string colliderName)
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.InvalidationAttackCollider>()
                .Publish(this.actor, ActorEvents.InvalidationAttackCollider.Get(colliderName));
        }
    }
}
