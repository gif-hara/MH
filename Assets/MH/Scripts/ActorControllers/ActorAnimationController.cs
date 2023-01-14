using Cysharp.Threading.Tasks;
using MessagePipe;

namespace MH
{
    /// <summary>
    /// </summary>
    public sealed class ActorAnimationController : IActorController
    {
        private AnimationController animationController;

        private ActorAnimationData data;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.animationController = actorDependencyInjector.AnimationController;
            this.data = spawnData.animationData;
            this.animationController.Time = actor.TimeController.Time;
            this.animationController.SetSpeed(actor.TimeController.Time.totalTimeScale);
            MessageBroker.GetSubscriber<Time, TimeEvents.UpdatedTimeScale>()
                .Subscribe(actor.TimeController.Time, x =>
                {
                    animationController.SetSpeed(actor.TimeController.Time.totalTimeScale);
                });
        }

        public void PlayIdle()
        {
            animationController.Play(this.data.GetAnimationBlendData("Idle"));
        }

        public void PlayRun()
        {
            animationController.Play(this.data.GetAnimationBlendData("Run"));
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
            return animationController.PlayAsync(this.data.GetAnimationBlendData("Dodge"));
        }
    }
}
