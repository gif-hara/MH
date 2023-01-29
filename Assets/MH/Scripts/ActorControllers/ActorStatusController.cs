using Cysharp.Threading.Tasks;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorStatusController : IActorController
    {
        private Actor actor;

        public ActorStatus BaseStatus { private set; get; }

        private readonly AsyncReactiveProperty<int> hitPointMax = new(0);

        private readonly AsyncReactiveProperty<int> hitPoint = new(0);

        public IAsyncReactiveProperty<int> HitPointMax => this.hitPointMax;

        public IAsyncReactiveProperty<int> HitPoint => this.hitPoint;

        public bool IsDead => this.hitPoint.Value <= 0;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.actor = actor;
            this.BaseStatus = new ActorStatus(spawnData.actorStatus);
            this.hitPointMax.Value = this.BaseStatus.hitPoint;
            this.hitPoint.Value = this.BaseStatus.hitPoint;
        }

        public void ReceiveDamage(int damage)
        {
            if (this.IsDead)
            {
                return;
            }

            this.hitPoint.Value -= damage;
            MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedDamage>()
                .Publish(this.actor, ActorEvents.ReceivedDamage.Get(damage));
            if (this.IsDead)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.Died>()
                    .Publish(this.actor, ActorEvents.Died.Get());
            }
        }
    }
}
