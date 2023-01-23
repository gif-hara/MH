using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MH.ActorControllers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// プレイヤーのネットワークを制御するクラス
    /// </summary>
    public sealed class PlayerNetworkBehaviour : NetworkBehaviour
    {
        [SerializeField]
        private ActorSpawnDataScriptableObject actorSpawnData;

        [SerializeField]
        private Actor actorPrefab;

        [SerializeField]
        private CameraController cameraControllerPrefab;

        private readonly NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

        private readonly NetworkVariable<float> networkRotationY = new();

        private Actor actor;

        public Vector3 NetworkPosition => this.networkPosition.Value;

        public float NetworkRotation => this.networkRotationY.Value;

        public override void OnNetworkSpawn()
        {
            var playerActorCommonData = PlayerActorCommonData.Instance;
            var ct = this.GetCancellationTokenOnDestroy();
            if (this.IsOwner)
            {
                Instantiate(this.cameraControllerPrefab, this.transform);
                var spawnData = this.actorSpawnData.data;
                spawnData.actorAI = new ActorAIPlayer(this);
                this.actor = this.actorPrefab.Spawn(spawnData, Vector3.zero, Quaternion.identity);
                this.actor.transform.SetParent(this.transform);
            }
            else
            {
                var spawnData = this.actorSpawnData.data;
                spawnData.actorAI = null;
                this.actor = this.actorPrefab.Spawn(spawnData, Vector3.zero, Quaternion.identity);
                this.actor.transform.SetParent(this.transform);

                this.GetAsyncUpdateTrigger()
                    .Subscribe(_ =>
                    {
                        // 座標の更新
                        {
                            var difference = this.networkPosition.Value - this.actor.transform.localPosition;
                            var threshold = playerActorCommonData.WarpPositionThreshold;
                            if (difference.sqrMagnitude > threshold * threshold)
                            {
                                this.actor.PostureController.Warp(this.networkPosition.Value);
                            }
                            else
                            {
                                var sqrMagnitude = difference.sqrMagnitude;
                                threshold = playerActorCommonData.MoveSpeed * playerActorCommonData.MoveSpeed;
                                if (sqrMagnitude >= threshold)
                                {
                                    var direction = difference.normalized;
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.actor, ActorEvents.RequestMove.Get(direction * playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime));
                                }
                                else if (sqrMagnitude < threshold && sqrMagnitude > 0.01f)
                                {
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.actor, ActorEvents.RequestMove.Get(difference * playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime));
                                }
                            }
                        }
                        // 回転の更新
                        {
                            var rotation = Quaternion.Lerp(
                                this.actor.transform.localRotation,
                                Quaternion.Euler(0.0f, this.networkRotationY.Value, 0.0f),
                                playerActorCommonData.RotationSpeed * this.actor.TimeController.Time.deltaTime
                                );
                            MessageBroker.GetPublisher<Actor, ActorEvents.RequestRotation>()
                                .Publish(this.actor, ActorEvents.RequestRotation.Get(rotation));
                        }
                    })
                    .AddTo(ct);
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
            this.SubmitAttackMotionNameServerRpc(new FixedString32Bytes(motionName));
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
        private void SubmitAttackMotionNameServerRpc(FixedString32Bytes motionName, ServerRpcParams rpcParams = default)
        {
            this.SubmitAttackMotionNameClientRpc(motionName);
        }

        [ClientRpc]
        private void SubmitAttackMotionNameClientRpc(FixedString32Bytes motionName, ClientRpcParams rpcParams = default)
        {
            if (this.IsOwner)
            {
                return;
            }
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttackNetwork>()
                .Publish(this.actor, ActorEvents.RequestAttackNetwork.Get(motionName.Value));
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
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestDodgeNetwork>()
                .Publish(this.actor, ActorEvents.RequestDodgeNetwork.Get(data.ToInvokeData));
        }
    }
}
