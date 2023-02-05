using System;
using DG.Tweening;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// </summary>
    public sealed class ActorDodgeController : IActorController
    {
        private Actor actor;

        private InvokeData invokeData;

        private float dodgeStaminaAmount;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            this.dodgeStaminaAmount = spawnData.dodgeStaminaAmount;
        }

        public void Ready(InvokeData invokeData)
        {
            this.invokeData = invokeData;
        }

        public async void Invoke()
        {
            this.actor.StatusController.UseStamina(this.dodgeStaminaAmount);
            var rotation = Quaternion.LookRotation(this.invokeData.direction);
            this.actor.PostureController.Rotate(rotation, true);
            this.actor.PostureController.CanRotation = false;
            var tween = DOTween.To(
                    () => this.invokeData.speed,
                    x =>
                    {
                        this.actor.PostureController.Move(
                            this.invokeData.direction * x * this.actor.TimeController.Time.deltaTime,
                            true
                            );
                    },
                    0.0f,
                    this.invokeData.duration
                    )
                .SetEase(this.invokeData.ease);

            try
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginDodge>()
                    .Publish(this.actor, ActorEvents.BeginDodge.Get(this.invokeData));

                await this.actor.AnimationController.PlayAsync("Dodge");

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
