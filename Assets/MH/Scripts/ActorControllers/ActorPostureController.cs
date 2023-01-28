using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の姿勢を制御するクラス
    /// </summary>
    public sealed class ActorPostureController : IActorController
    {
        private readonly Vector3 gravity = new(0.0f, -60.0f, 0.0f);

        private Actor actor;

        private Vector3 currentGravity;

        private Vector3 currentMoveVector;

        private bool isMoving = true;

        private OpenCharacterController openCharacterController;

        public bool CanRotation { set; get; } = true;

        public bool CanMove { set; get; } = true;

        public float Radius => this.openCharacterController.scaledRadius;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.actor = actor;
            this.openCharacterController = actorDependencyInjector.OpenCharacterController;

            var ct = this.actor.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestSetForce>().Subscribe(this.actor, x =>
                {
                    this.SetForce(x.Force);
                })
                .AddTo(ct);

            this.actor.GetAsyncLateUpdateTrigger()
                .Subscribe(_ =>
                {
                    if (this.currentMoveVector.sqrMagnitude > 0.0f)
                    {
                        if (!this.isMoving)
                        {
                            this.isMoving = true;
                            MessageBroker.GetPublisher<Actor, ActorEvents.BeginMove>()
                                .Publish(this.actor, ActorEvents.BeginMove.Get());
                        }
                    }
                    else
                    {
                        if (this.isMoving)
                        {
                            this.isMoving = false;
                            MessageBroker.GetPublisher<Actor, ActorEvents.EndMove>()
                                .Publish(this.actor, ActorEvents.EndMove.Get());
                        }
                    }

                    this.currentGravity += this.gravity * this.actor.TimeController.Time.deltaTime;

                    var totalVector = this.currentMoveVector + (this.currentGravity * this.actor.TimeController.Time.deltaTime);

                    this.openCharacterController.Move(totalVector);
                    this.currentMoveVector = Vector3.zero;

                    if (this.openCharacterController.isGrounded)
                    {
                        this.currentGravity = Vector3.zero;
                    }
                })
                .AddTo(ct);
        }

        public void Move(Vector3 moveVector, bool isForce = false)
        {
            if (!isForce && !this.CanMove)
            {
                return;
            }

            this.currentMoveVector += moveVector;
        }

        public void AddForce(Vector3 force)
        {
            this.currentGravity += force;
        }

        public void SetForce(Vector3 force)
        {
            this.currentGravity = force;
        }

        public void Warp(Vector3 position)
        {
            this.openCharacterController.SetPosition(position, true);
            this.currentGravity = Vector3.zero;
        }

        public void Rotate(Quaternion rotation, bool isForce = false)
        {
            if (!isForce && !this.CanRotation)
            {
                return;
            }
            this.actor.transform.localRotation = rotation;
        }
    }
}
