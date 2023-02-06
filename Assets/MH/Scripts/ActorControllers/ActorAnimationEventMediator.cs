using MH.ProjectileSystems;
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
        /// 回転可能な状態にする
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void RotationAccept()
        {
            this.actor.PostureController.CanRotation = true;
        }

        /// <summary>
        /// 回転不可能な状態にする
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void RotationReject()
        {
            this.actor.PostureController.CanRotation = false;
        }

        public void MoveAccept()
        {
            this.actor.PostureController.CanMove = true;
        }

        public void MoveReject()
        {
            this.actor.PostureController.CanMove = false;
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
        public void ValidationAttackCollider(Object obj)
        {
            var data = (ValidationAttackColliderData)obj;
            Assert.IsNotNull(data, $"{this.actor.name}の攻撃データが存在しません");
            MessageBroker.GetPublisher<Actor, ActorEvents.ValidationAttackCollider>()
                .Publish(this.actor, ActorEvents.ValidationAttackCollider.Get(data));
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

        /// <summary>
        /// 物理的な力を与えます
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void RequestSetForce(Object obj)
        {
            var scriptableVector3 = (ScriptableVector3)obj;
            Assert.IsNotNull(scriptableVector3);
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestSetForce>()
                .Publish(this.actor, ActorEvents.RequestSetForce.Get(scriptableVector3.vector3));
        }

        /// <summary>
        /// 無敵を開始する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void BeginInvincible(float durationSeconds)
        {
            if (this.actor == null)
            {
                Debug.LogWarning("actor is null.");
                return;
            }

            this.actor.StatusController.BeginInvincibleAsync(durationSeconds).Forget();
        }

        /// <summary>
        /// アニメーションを再生する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void PlayAnimation(string motionName)
        {
            if (this.actor == null)
            {
                Debug.LogWarning("actor is null.");
                return;
            }

            this.actor.AnimationController.Play(motionName);
        }

        /// <summary>
        /// 回復を開始する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void BeginRecovery()
        {
            if (this.actor == null)
            {
                Debug.LogWarning("actor is null.");
                return;
            }

            this.actor.StatusController.BeginRecovery();
        }

        /// <summary>
        /// <see cref="Projectile"/>を生成する
        /// </summary>
        /// <remarks>
        /// この関数はアニメーションイベントで実行されます
        /// </remarks>
        public void SpawnProjectile(Object obj)
        {
            var data = (SpawnProjectileData)obj;
            var t = this.actor.transform;
            var position = t.TransformPoint(data.PositionOffset);
            var rotation = t.rotation * Quaternion.Euler(data.RotationOffset);
            data.ProjectilePrefab.Spawn(data.Data, this.actor, position, rotation);
        }
    }
}
