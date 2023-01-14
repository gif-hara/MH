namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>のモデルを制御するクラス
    /// </summary>
    public sealed class ActorModelController : IActorController
    {
        public BoneHolder BoneHolder { private set; get; }

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.BoneHolder = actorDependencyInjector.BoneHolder;
        }
    }
}
