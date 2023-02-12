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

        private readonly NetworkVariable<float> networkHitPoint = new();

        private readonly NetworkVariable<bool> networkGuarding = new();

        private NetworkList<PartDataNetworkVariable> networkPartDataList;

        protected Actor actor;

        public Vector3 NetworkPosition => this.networkPosition.Value;

        public float NetworkRotation => this.networkRotationY.Value;

        void Awake()
        {
            // なぜかこのタイミングでnewしないとコンパイルエラーになる
            // https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1367
            this.networkPartDataList = new NetworkList<PartDataNetworkVariable>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            this.actor = this.actorPrefab.Spawn(this.actorSpawnData.data, this);
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
            }

            this.networkHitPoint.OnValueChanged += this.OnChangedHitPoint;
            this.networkPartDataList.OnListChanged += this.OnChangedPartDataList;
            this.networkGuarding.OnValueChanged += this.OnChangedGuarding;
        }

        private void OnChangedPartDataList(NetworkListEvent<PartDataNetworkVariable> changeEvent)
        {
            this.actor.StatusController.SyncPartDataList(this.networkPartDataList);
        }

        private void OnChangedHitPoint(float previousValue, float newValue)
        {
            this.actor.StatusController.SyncHitPoint(this.networkHitPoint);
        }

        private void OnChangedGuarding(bool previousValue, bool newValue)
        {
            this.actor.GuardController.SyncGuarding(this.networkGuarding);
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

        public void SubmitSetGuarding(bool newGuarding)
        {
            this.SubmitSetGuardingServerRpc(newGuarding);
        }

        public void SubmitRequestJudgeResult(Define.BattleResult result)
        {
            this.SubmitRequestJudgeResultServerRpc(result);
        }

        public void SyncHitPoint(float hitPoint)
        {
            this.SubmitSyncHitPointServerRpc(hitPoint);
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

        [ServerRpc]
        private void SubmitSetGuardingServerRpc(bool newGuarding, ServerRpcParams rpcParams = default)
        {
            this.networkGuarding.Value = newGuarding;
        }

        [ServerRpc]
        private void SubmitRequestJudgeResultServerRpc(Define.BattleResult result, ServerRpcParams rpcParams = default)
        {
            this.SubmitRequestJudgeResultClientRpc(result);
        }

        [ClientRpc]
        private void SubmitRequestJudgeResultClientRpc(Define.BattleResult result, ClientRpcParams rpcParams = default)
        {
            MessageBroker.GetPublisher<BattleEvents.RequestJudgeResult>()
                .Publish(BattleEvents.RequestJudgeResult.Get(result));
        }

        [ServerRpc]
        private void SubmitSyncHitPointServerRpc(float hitPoint, ServerRpcParams rpcParams = default)
        {
            this.networkHitPoint.Value = hitPoint;
        }
    }
}
