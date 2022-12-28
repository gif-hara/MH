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

        public void BeginRotation()
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.AcceptRequestRotation>()
                .Publish(this.actor, ActorEvents.AcceptRequestRotation.Get());
        }
    }
}
