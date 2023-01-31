using MH.ActorControllers;
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

        private readonly NetworkVariable<int> networkHitPoint = new();

        protected Actor actor;

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

        [ServerRpc]
        private void SubmitGaveDamageServerRpc(ulong networkObjectId, int damage, ServerRpcParams rpcParams = default)
        {
            var target = ActorManager.GetActorFromNetworkObjectId(networkObjectId);
            target.NetworkController.NetworkBehaviour.networkHitPoint.Value -= damage;
        }
    }
}
