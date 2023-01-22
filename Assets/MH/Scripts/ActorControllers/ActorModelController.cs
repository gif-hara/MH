namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>のモデルを制御するクラス
    /// </summary>
    public sealed class ActorModelController : IActorController
    {
        public ModelDataHolder ModelDataHolder { private set; get; }

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.ModelDataHolder = actorDependencyInjector.ModelDataHolder;
        }
    }
}
