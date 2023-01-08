using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MessagePipe;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorAnimationController : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;
        
        [SerializeField]
        private AnimationController animationController;

        [SerializeField]
        private AnimationBlendData idle;

        [SerializeField]
        private AnimationBlendData run;

        [SerializeField]
        private AnimationBlendData dodge;
        
        private void Start()
        {
            this.animationController.Time = this.actor.TimeController.Time;
            this.animationController.SetSpeed(this.actor.TimeController.Time.totalTimeScale);
            MessageBroker.GetSubscriber<Time, TimeEvents.UpdatedTimeScale>()
                .Subscribe(this.actor.TimeController.Time, x =>
                {
                    this.animationController.SetSpeed(this.actor.TimeController.Time.totalTimeScale);
                });
        }

        public void PlayIdle()
        {
            this.animationController.Play(this.idle);
        }

        public void PlayRun()
        {
            this.animationController.Play(this.run);
        }

        public void Play(AnimationBlendData data)
        {
            this.animationController.Play(data);
        }

        public UniTask<AnimationController.CompleteType> PlayAsync(AnimationBlendData data)
        {
            return this.animationController.PlayAsync(data);
        }

        public UniTask<AnimationController.CompleteType> PlayDodgeAsync()
        {
            return this.animationController.PlayAsync(this.dodge);
        }
    }
}
