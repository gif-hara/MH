using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
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
            animationController.Time = actor.TimeController.Time;
            animationController.SetSpeed(actor.TimeController.Time.totalTimeScale);
            MessageBroker.GetSubscriber<Time, TimeEvents.UpdatedTimeScale>()
                .Subscribe(actor.TimeController.Time, x =>
                {
                    animationController.SetSpeed(actor.TimeController.Time.totalTimeScale);
                });
        }

        public void PlayIdle()
        {
            animationController.Play(idle);
        }

        public void PlayRun()
        {
            animationController.Play(run);
        }

        public void Play(AnimationBlendData data)
        {
            animationController.Play(data);
        }

        public UniTask<AnimationController.CompleteType> PlayAsync(AnimationBlendData data)
        {
            return animationController.PlayAsync(data);
        }

        public UniTask<AnimationController.CompleteType> PlayDodgeAsync()
        {
            return animationController.PlayAsync(dodge);
        }
    }
}
