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
        
        public ActorDodgeController(Actor actor)
        {
            this.actor = actor;
        }

        public void Invoke(
            Vector3 direction,
            float speed,
            float duration,
            Ease ease
            )
        {
            this.actor.PostureController.Rotate(Quaternion.LookRotation(direction));
            DOTween.To(
                    () => speed,
                    x =>
                    {
                        this.actor.PostureController.Move(direction * x * Time.deltaTime);
                    },
                    0.0f,
                    duration
                    )
                .SetEase(ease);
        }
    }
}
