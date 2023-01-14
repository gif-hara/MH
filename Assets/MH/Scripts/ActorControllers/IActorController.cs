namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>を制御するインターフェイス
    /// </summary>
    public interface IActorController
    {
        /// <summary>
        /// アタッチする
        /// </summary>
        void Setup(Actor actor, ActorSpawnData spawnData);
    }
}
