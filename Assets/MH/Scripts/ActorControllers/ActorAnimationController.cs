using Cysharp.Threading.Tasks;
using MessagePipe;

namespace MH.ActorControllers
{
    /// <summary>
    /// </summary>
    public sealed class ActorAnimationController : IActorController
    {
        private AnimationController animationController;

        private ActorAnimationData data;

        public string CurrentAnimationName { private set; get; }

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

        public void Play(string animationName)
        {
            this.CurrentAnimationName = animationName;
            this.animationController.Play(this.data.GetAnimationBlendData(animationName));
        }

        public UniTask<AnimationController.CompleteType> PlayAsync(string animationName)
        {
            this.CurrentAnimationName = animationName;
            return this.animationController.PlayAsync(this.data.GetAnimationBlendData(animationName));
        }
    }
}
