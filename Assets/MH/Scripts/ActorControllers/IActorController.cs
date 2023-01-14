namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>を制御するインターフェイス
    /// </summary>
    public interface IActorController
    {
        /// <summary>
        /// セットアップする
        /// </summary>
        void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData);
    }
}
