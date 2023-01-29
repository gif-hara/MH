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
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestNetworkNewPosture>()
                .Subscribe(this.actor, _ =>
                {
                    var t = this.actor.transform;
                    this.SubmitPostureServerRpc(t.position, t.rotation.eulerAngles.y);
                })
                .AddTo(ct);
        }

        public void SpawnToNetwork()
        {
            this.networkObject.Spawn(true);
        }

        [ServerRpc]
        private void SubmitPostureServerRpc(Vector3 newPosition, float newRotationY, ServerRpcParams rpcParams = default)
        {
            this.SubmitPostureClientRpc(newPosition, newRotationY);
        }

        [ClientRpc]
        private void SubmitPostureClientRpc(Vector3 newPosition, float newRotationY, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }

            this.actor.PostureController.Warp(newPosition);
            this.actor.PostureController.Rotate(Quaternion.Euler(0.0f, newRotationY, 0.0f));
        }
    }
}
