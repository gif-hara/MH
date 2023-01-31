using MH.ActorControllers;
using Unity.Collections;
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

        private readonly NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

        private readonly NetworkVariable<float> networkRotationY = new();

        public Vector3 NetworkPosition => this.networkPosition.Value;

        public float NetworkRotation => this.networkRotationY.Value;

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

        public void SubmitPosition(Vector3 newPosition)
        {
            this.SubmitPositionServerRpc(newPosition);
        }

        public void SubmitRotation(float newRotationY)
        {
            this.SubmitRotationYServerRpc(newRotationY);
        }

        public void SubmitAttackMotionName(string motionName)
        {
            this.SubmitRequestUniqueMotionServerRpc(new FixedString32Bytes(motionName));
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
        private void SubmitPositionServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
        {
            this.networkPosition.Value = newPosition;
        }

        [ServerRpc]
        private void SubmitRotationYServerRpc(float newRotationY, ServerRpcParams rpcParams = default)
        {
            this.networkRotationY.Value = newRotationY;
        }

        [ServerRpc]
        private void SubmitRequestUniqueMotionServerRpc(FixedString32Bytes motionName, ServerRpcParams rpcParams = default)
        {
            this.SubmitRequestUniqueMotionClientRpc(motionName);
        }

        [ClientRpc]
        private void SubmitRequestUniqueMotionClientRpc(FixedString32Bytes motionName, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }
            MessageBroker.GetPublisher<Actor, ActorEvents.NetworkRequestUniqueMotion>()
                .Publish(this.actor, ActorEvents.NetworkRequestUniqueMotion.Get(motionName.Value));
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
