using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorDodgeController
    {
        private readonly Actor actor;

        private Vector3 direction;

        private float speed;

        private float duration;

        private Ease ease;
        
        public ActorDodgeController(Actor actor)
        {
            this.actor = actor;
        }

        public void Ready(
            Vector3 direction,
            float speed,
            float duration,
            Ease ease
            )
        {
            this.direction = direction;
            this.speed = speed;
            this.duration = duration;
            this.ease = ease;
        }

        public async void Invoke()
        {
            this.actor.PostureController.Rotate(Quaternion.LookRotation(this.direction));
            var tween = DOTween.To(
                    () => this.speed,
                    x =>
                    {
                        this.actor.PostureController.Move(this.direction * x * this.actor.TimeController.Time.deltaTime);
                    },
                    0.0f,
                    this.duration
                    )
                .SetEase(this.ease);

            try
            {
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
    }
}
