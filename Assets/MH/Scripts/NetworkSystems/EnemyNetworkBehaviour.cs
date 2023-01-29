using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// 敵を管理する<see cref="NetworkBehaviour"/>
    /// </summary>
    public sealed class EnemyNetworkBehaviour : NetworkBehaviour
    {
        [SerializeField]
        private NetworkObject networkObject;

        [SerializeField]
        private Actor actorPrefab;

        [SerializeField]
        private ActorSpawnDataScriptableObject spawnData;

        private Actor actor;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            this.actor = this.actorPrefab.Spawn(this.spawnData.data, this.transform);
            this.actor.transform.SetParent(this.transform, false);

            var ct = this.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestSubmitNewThinkData>()
                .Subscribe(this.actor, x =>
                {
                    this.SubmitPostureServerRpc(x.Position, x.RotationY, x.Seed);
                })
                .AddTo(ct);
        }

        public void SpawnToNetwork()
        {
            this.networkObject.Spawn(true);
        }

        [ServerRpc]
        private void SubmitPostureServerRpc(Vector3 newPosition, float newRotationY, int thinkSeed, ServerRpcParams rpcParams = default)
        {
            this.SubmitPostureClientRpc(newPosition, newRotationY, thinkSeed);
        }

        [ClientRpc]
        private void SubmitPostureClientRpc(Vector3 newPosition, float newRotationY, int thinkSeed, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }

            MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedNewThinkData>()
                .Publish(this.actor, ActorEvents.ReceivedNewThinkData.Get(newPosition, newRotationY, thinkSeed));
        }
    }
}
