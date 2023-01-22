namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorAIController : IActorController
    {
        private IActorAI ai;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.ai = spawnData.actorAI;
            this.ai?.Attach(actor);
        }
    }
}
