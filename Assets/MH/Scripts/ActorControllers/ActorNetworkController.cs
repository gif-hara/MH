using Unity.Netcode;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorNetworkController : IActorController
    {
        public NetworkBehaviour NetworkBehaviour { set; private get; }

        public bool IsOwner => this.NetworkBehaviour.IsOwner;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
        }
    }
}
