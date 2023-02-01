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

        private readonly NetworkList<PartDataNetworkVariable> networkPartDataList = new();

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
                this.networkPartDataList.Clear();
                foreach (var (key, value) in this.actor.StatusController.CurrentEndurances)
                {
                    this.networkPartDataList.Add(new PartDataNetworkVariable
                    {
                        partType = key,
                        endurance = value
                    });
                }
            }
            else
            {
                this.actor.StatusController.SyncHitPoint(this.networkHitPoint);
                Debug.Log($"OnNetworkSpawn {this.actor} hitPoint = {this.networkHitPoint.Value}");
            }

            this.networkHitPoint.OnValueChanged += OnChangedHitPoint;
            this.networkPartDataList.OnListChanged += OnChangedPartDataList;
        }

        private void OnChangedPartDataList(NetworkListEvent<PartDataNetworkVariable> changeEvent)
        {
            this.actor.StatusController.SyncPartDataList(this.networkPartDataList);
        }

        private void OnChangedHitPoint(int previousValue, int newValue)
        {
            this.actor.StatusController.SyncHitPoint(this.networkHitPoint);
            Debug.Log($"{actor.name} HitPoint = {newValue}");
        }

        public void SubmitGaveDamage(ulong networkObjectId, int damage, Define.PartType partType)
        {
            this.SubmitGaveDamageServerRpc(networkObjectId, damage, partType);
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
        private void SubmitGaveDamageServerRpc(ulong networkObjectId, int damage, Define.PartType partType, ServerRpcParams rpcParams = default)
        {
            var target = ActorManager.GetActorFromNetworkObjectId(networkObjectId);
            var n = target.NetworkController.NetworkBehaviour;
            n.networkHitPoint.Value -= damage;
            for (var i = 0; i < n.networkPartDataList.Count; i++)
            {
                if (n.networkPartDataList[i].partType == partType)
                {
                    var p = n.networkPartDataList[i];
                    p.endurance += damage;
                    p.opposePosition = target.transform.position - this.actor.transform.position;
                    n.networkPartDataList[i] = p;
                    break;
                }
            }
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
