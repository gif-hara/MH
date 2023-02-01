using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// プレイヤーのネットワークを制御するクラス
    /// </summary>
    public sealed class PlayerNetworkBehaviour : ActorNetworkBehaviour
    {
        [SerializeField]
        private CameraController cameraControllerPrefab;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (this.IsOwner)
            {
                Instantiate(this.cameraControllerPrefab, this.transform);
                OwnerPlayerActorBehaviour.Attach(this.actor, this);
            }
            else
            {
                GhostPlayerActorBehaviour.Attach(this.actor, this);
            }
        }

        public void SubmitBeginDodge(ActorDodgeController.InvokeData data)
        {
            this.SubmitBeginDodgeServerRpc(new DodgeNetworkVariable
            {
                direction = data.direction,
                duration = data.duration,
                ease = data.ease,
                speed = data.speed
            });
        }

        [ServerRpc]
        private void SubmitBeginDodgeServerRpc(DodgeNetworkVariable data, ServerRpcParams rpcParams = default)
        {
            this.SubmitBeginDodgeClientRpc(data);
        }

        [ClientRpc]
        private void SubmitBeginDodgeClientRpc(DodgeNetworkVariable data, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }
            MessageBroker.GetPublisher<Actor, ActorEvents.NetworkRequestDodge>()
                .Publish(this.actor, ActorEvents.NetworkRequestDodge.Get(data.ToInvokeData));
        }
    }
}
