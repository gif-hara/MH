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
        private AnimationClip idleClip;

        [SerializeField]
        private AnimationClip runClip;

        [SerializeField]
        private AnimationClip dodgeClip;

        [SerializeField]
        private float blendSeconds;

        private void Start()
        {
            this.animationController.Time = this.actor.Time;
            this.animationController.SetSpeed(this.actor.Time.totalTimeScale);
            MessageBroker.GetSubscriber<Time, TimeEvents.UpdatedTimeScale>()
                .Subscribe(this.actor.Time, x =>
                {
                    this.animationController.SetSpeed(this.actor.Time.totalTimeScale);
                });
        }

        public void PlayIdle()
        {
            this.animationController.Play(this.idleClip, this.blendSeconds);
        }

        public void PlayRun()
        {
            this.animationController.Play(this.runClip, this.blendSeconds);
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
            return this.animationController.PlayAsync(this.dodgeClip, this.blendSeconds);
        }
    }
}
