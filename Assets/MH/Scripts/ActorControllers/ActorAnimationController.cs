using System;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorAnimationController : MonoBehaviour
    {
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

        public void PlayIdle()
        {
            this.animationController.Play(this.idleClip, this.blendSeconds);
        }

        public void PlayRun()
        {
            this.animationController.Play(this.runClip, this.blendSeconds);
        }

        public IObservable<AnimationController.CompleteType> PlayDodgeAsync()
        {
            return this.animationController.PlayAsync(this.dodgeClip, this.blendSeconds);
        }
    }
}
