namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorStatusController : IActorController
    {
        public ActorStatus BaseStatus { private set; get; }

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.BaseStatus = spawnData.actorStatus;
        }
    }
}
