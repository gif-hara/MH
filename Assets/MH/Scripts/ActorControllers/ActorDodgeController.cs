using System;
using DG.Tweening;
using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorDodgeController : IActorController
    {
        private Actor actor;

        private InvokeData invokeData;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
        }

        public void Ready(InvokeData invokeData)
        {
            this.invokeData = invokeData;
        }

        public async void Invoke()
        {
            var rotation = Quaternion.LookRotation(this.invokeData.direction);
            this.actor.PostureController.Rotate(rotation);
            var tween = DOTween.To(
                    () => this.invokeData.speed,
                    x =>
                    {
                        this.actor.PostureController.Move(this.invokeData.direction * x * this.actor.TimeController.Time.deltaTime);
                    },
                    0.0f,
                    this.invokeData.duration
                    )
                .SetEase(this.invokeData.ease);

            try
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginDodge>()
                    .Publish(this.actor, ActorEvents.BeginDodge.Get(this.invokeData));

                await this.actor.AnimationController.PlayDodgeAsync();

                MessageBroker.GetPublisher<Actor, ActorEvents.EndDodge>()
                    .Publish(this.actor, ActorEvents.EndDodge.Get());
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                tween.Kill();
            }
        }

        public struct InvokeData
        {
            public Vector3 direction;

            public float duration;

            public Ease ease;

            public float speed;
        }
    }
}
