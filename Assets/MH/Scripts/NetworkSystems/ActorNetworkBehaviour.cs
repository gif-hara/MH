using MH.ActorControllers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// </summary>
    public abstract class ActorNetworkBehaviour : NetworkBehaviour
    {
        [SerializeField]
        private ActorSpawnDataScriptableObject actorSpawnData;

        [SerializeField]
        private Actor actorPrefab;

        private readonly NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

        private readonly NetworkVariable<float> networkRotationY = new();

        private readonly NetworkVariable<int> networkHitPoint = new();

        protected Actor actor;

        public Vector3 NetworkPosition => this.networkPosition.Value;

        public float NetworkRotation => this.networkRotationY.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            this.actor = this.actorPrefab.Spawn(this.actorSpawnData.data, this, this.transform);
            this.actor.transform.SetParent(this.transform, false);
            if (this.IsHost)
            {
                this.networkHitPoint.Value = this.actor.StatusController.HitPoint.Value;
            }
            else
            {
                this.actor.StatusController.SyncHitPoint(this.networkHitPoint);
                Debug.Log($"OnNetworkSpawn {this.actor} hitPoint = {this.networkHitPoint.Value}");
            }

            this.networkHitPoint.OnValueChanged += OnChangedHitPoint;
        }

        private void OnChangedHitPoint(int previousValue, int newValue)
        {
            this.actor.StatusController.SyncHitPoint(this.networkHitPoint);
            Debug.Log($"{actor.name} HitPoint = {newValue}");
        }

        public void SubmitGaveDamage(ulong networkObjectId, int damage)
        {
            this.SubmitGaveDamageServerRpc(networkObjectId, damage);
        }

        public void SubmitPosition(Vector3 newPosition)
        {
            this.SubmitPositionServerRpc(newPosition);
        }

        public void SubmitRotation(float newRotationY)
        {
            this.SubmitRotationYServerRpc(newRotationY);
        }

        public void SubmitRequestUniqueMotion(string motionName)
        {
            this.SubmitRequestUniqueMotionServerRpc(new FixedString32Bytes(motionName));
        }

        [ServerRpc]
        private void SubmitGaveDamageServerRpc(ulong networkObjectId, int damage, ServerRpcParams rpcParams = default)
        {
            var target = ActorManager.GetActorFromNetworkObjectId(networkObjectId);
            target.NetworkController.NetworkBehaviour.networkHitPoint.Value -= damage;
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
    }
}
