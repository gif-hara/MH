using MH.NetworkSystems;

namespace MH.ActorControllers
{
    /// <summary>
    /// </summary>
    public sealed class ActorNetworkController : IActorController
    {
        public ActorNetworkBehaviour NetworkBehaviour { set; get; }

        public bool IsOwner => this.NetworkBehaviour.IsOwner;

        public ulong NetworkObjectId => this.NetworkBehaviour.NetworkObjectId;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
        }
    }
}
