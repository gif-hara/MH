using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
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
        private PlayerActorCommonData playerActorCommonData;

        [SerializeField]
        private ActorSpawnDataScriptableObject actorSpawnData;

        [SerializeField]
        private Actor actorPrefab;

        [SerializeField]
        private CameraController cameraControllerPrefab;

        [SerializeField]
        private float sendPositionThreshold;

        [SerializeField]
        private float sendRotationYThreshold;

        [SerializeField]
        private float warpPositionThreshold;

        private readonly NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

        private readonly NetworkVariable<float> networkRotationY = new();

        private Actor actor;

        public override void OnNetworkSpawn()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            if (this.IsOwner)
            {
                Instantiate(this.cameraControllerPrefab, this.transform);
                var spawnData = this.actorSpawnData.data;
                spawnData.actorAI = new ActorAIPlayer(this.playerActorCommonData);
                this.actor = this.actorPrefab.Spawn(spawnData, Vector3.zero, Quaternion.identity);
                this.actor.transform.SetParent(this.transform);
                UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ =>
                    {
                        this.TrySubmitPosition();
                        this.TrySubmitRotationY();
                    })
                    .AddTo(ct);

                MessageBroker.GetSubscriber<Actor, ActorEvents.BeginAttack>()
                    .Subscribe(this.actor, x =>
                    {
                        this.SubmitAttackMotionNameServerRpc( new FixedString32Bytes(x.MotionName));
                    })
                    .AddTo(ct);

                MessageBroker.GetSubscriber<Actor, ActorEvents.BeginDodge>()
                    .Subscribe(this.actor, x =>
                    {
                        this.SubmitBeginDodgeServerRpc(new DodgeNetworkVariable
                        {
                            direction = x.Data.direction,
                            duration = x.Data.duration,
                            ease = x.Data.ease,
                            speed = x.Data.speed
                        });
                    })
                    .AddTo(ct);
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
                            if (difference.sqrMagnitude > this.warpPositionThreshold * this.warpPositionThreshold)
                            {
                                this.actor.PostureController.Warp(this.networkPosition.Value);
                            }
                            else
                            {
                                var sqrMagnitude = difference.sqrMagnitude;
                                var threshold = this.playerActorCommonData.MoveSpeed * this.playerActorCommonData.MoveSpeed;
                                if (sqrMagnitude >= threshold)
                                {
                                    var direction = difference.normalized;
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.actor, ActorEvents.RequestMove.Get(direction * this.playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime));
                                }
                                else if (sqrMagnitude < threshold && sqrMagnitude > 0.01f)
                                {
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.actor, ActorEvents.RequestMove.Get(difference * this.playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime));
                                }
                            }
                        }
                        // 回転の更新
                        {
                            var rotation = Quaternion.Lerp(
                                this.actor.transform.localRotation,
                                Quaternion.Euler(0.0f, this.networkRotationY.Value, 0.0f),
                                this.playerActorCommonData.RotationSpeed * this.actor.TimeController.Time.deltaTime
                                );
                            MessageBroker.GetPublisher<Actor, ActorEvents.RequestRotation>()
                                .Publish(this.actor, ActorEvents.RequestRotation.Get(rotation));
                        }
                    })
                    .AddTo(ct);
            }
        }

        private void TrySubmitPosition()
        {
            var distance = (this.actor.transform.localPosition - this.networkPosition.Value).sqrMagnitude;
            if (distance > this.sendPositionThreshold * this.sendPositionThreshold)
            {
                this.SubmitPositionServerRpc(this.actor.transform.localPosition);
            }
        }

        private void TrySubmitRotationY()
        {
            var difference = this.networkRotationY.Value - this.actor.transform.localRotation.eulerAngles.y;
            difference = difference < 0 ? -difference : difference;
            if (difference > this.sendRotationYThreshold)
            {
                this.SubmitRotationYServerRpc(this.actor.transform.localRotation.eulerAngles.y);
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
