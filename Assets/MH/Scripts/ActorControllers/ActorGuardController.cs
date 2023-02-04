using Unity.Netcode;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>のガード処理を制御するクラス
    /// </summary>
    public sealed class ActorGuardController : IActorController
    {
        private Actor actor;

        /// <summary>
        /// ガード中であるか
        /// </summary>
        public bool Guarding { private set; get; }

        /// <summary>
        /// ガードが可能か
        /// </summary>
        public bool CanGuard { private set; get; }

        private float guardSuccessAngle;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            this.guardSuccessAngle = spawnData.guardSuccessAngle;
        }

        /// <summary>
        /// ガードを開始する
        /// </summary>
        public void Begin()
        {
            if (!this.CanGuard)
            {
                return;
            }

            var oldGuarding = this.Guarding;
            this.Guarding = true;

            if (oldGuarding != this.Guarding)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginGuard>()
                    .Publish(this.actor, ActorEvents.BeginGuard.Get());
            }
        }

        /// <summary>
        /// ガードを終了する
        /// </summary>
        public void End()
        {
            if (!this.Guarding)
            {
                return;
            }

            this.Guarding = false;
            MessageBroker.GetPublisher<Actor, ActorEvents.EndGuard>()
                .Publish(this.actor, ActorEvents.EndGuard.Get());
        }

        /// <summary>
        /// ガードが出来る状態に設定する
        /// </summary>
        public void Validate()
        {
            this.CanGuard = true;
        }

        /// <summary>
        /// ガードが出来ない状態に設定する
        /// </summary>
        public void Invalidate()
        {
            this.End();
            this.CanGuard = false;
        }

        /// <summary>
        /// ネットワーク上のガードフラグと同期を取る
        /// </summary>
        public void SyncGuarding(NetworkVariable<bool> networkGuarding)
        {
            if (networkGuarding.Value)
            {
                this.Begin();
            }
            else
            {
                this.End();
            }
        }

        /// <summary>
        /// 攻撃に対してガードが成功したか返す
        /// </summary>
        public bool IsSuccess(Vector3 attackPosition)
        {
            if (!this.Guarding)
            {
                return false;
            }
            var t = this.actor.transform;
            var diff = attackPosition - t.position;
            var result = Vector3.Dot(diff.normalized, t.forward);
            return result > this.guardSuccessAngle;
        }
    }
}
