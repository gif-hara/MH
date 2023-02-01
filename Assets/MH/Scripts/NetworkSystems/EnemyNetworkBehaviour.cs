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
    public sealed class EnemyNetworkBehaviour : ActorNetworkBehaviour
    {
        [SerializeField]
        private NetworkObject networkObject;

        private readonly NetworkVariable<ThinkDataNetworkVariable> networkThinkData = new();

        void Start()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                this.networkObject.Spawn(true);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            var ct = this.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestSubmitNewThinkData>()
                .Subscribe(this.actor, x =>
                {
                    this.SubmitPostureServerRpc(new ThinkDataNetworkVariable
                    {
                        position = x.Position,
                        rotationY = x.RotationY,
                        thinkSeed = x.Seed
                    });
                })
                .AddTo(ct);
            if (!NetworkManager.Singleton.IsHost)
            {
                var t = this.networkThinkData.Value;
                MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedNewThinkData>()
                    .Publish(this.actor, ActorEvents.ReceivedNewThinkData.Get(t.position, t.rotationY, t.thinkSeed));
            }
        }

        [ServerRpc]
        private void SubmitPostureServerRpc(ThinkDataNetworkVariable thinkData, ServerRpcParams rpcParams = default)
        {
            this.networkThinkData.Value = thinkData;
            this.SubmitPostureClientRpc(thinkData);
        }

        [ClientRpc]
        private void SubmitPostureClientRpc(ThinkDataNetworkVariable thinkData, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }

            MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedNewThinkData>()
                .Publish(this.actor, ActorEvents.ReceivedNewThinkData.Get(thinkData.position, thinkData.rotationY, thinkData.thinkSeed));
        }
    }
}
