namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorStatusController : IActorController
    {
        private Actor actor;

        public ActorStatus BaseStatus { private set; get; }

        public ActorStatus CurrentStatus { private set; get; }

        public bool IsDead => this.CurrentStatus.hitPoint <= 0;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.actor = actor;
            this.BaseStatus = new ActorStatus(spawnData.actorStatus);
            this.CurrentStatus = new ActorStatus(this.BaseStatus);
        }

        public void ReceiveDamage(int damage)
        {
            if (this.IsDead)
            {
                return;
            }

            this.CurrentStatus.hitPoint -= damage;
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
