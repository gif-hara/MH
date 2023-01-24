using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
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
            this.actor.PostureController.CanRotation = true;
        }

        /// <summary>
        /// 回転のリクエストの受付を終了する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void CloseRequestRotation()
        {
            this.actor.PostureController.CanRotation = false;
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

        public void RequestSetForce(Object obj)
        {
            var scriptableVector3 = (ScriptableVector3)obj;
            Assert.IsNotNull(scriptableVector3);
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestSetForce>()
                .Publish(this.actor, ActorEvents.RequestSetForce.Get(scriptableVector3.vector3));
        }
    }
}
