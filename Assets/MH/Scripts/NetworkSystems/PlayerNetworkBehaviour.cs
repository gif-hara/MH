using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
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
        private PlayerInputController playerInputControllerPrefab;

        [SerializeField]
        private float sendPositionThreshold;

        [SerializeField]
        private float sendRotationYThreshold;

        [SerializeField]
        private float warpPositionThreshold;

        private readonly NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

        private readonly NetworkVariable<float> networkRotationY = new();

        private Actor player;

        public override void OnNetworkSpawn()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            this.player = this.actorPrefab.Spawn(this.actorSpawnData.data, Vector3.zero, Quaternion.identity);
            this.player.transform.SetParent(this.transform);
            if (this.IsOwner)
            {
                var inputController = Instantiate(this.playerInputControllerPrefab, this.transform);
                inputController.Attach(this.player, this.playerActorCommonData);
                UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ =>
                    {
                        this.TrySubmitPosition();
                        this.TrySubmitRotationY();
                    })
                    .AddTo(ct);
            }
            else
            {
                this.GetAsyncUpdateTrigger()
                    .Subscribe(_ =>
                    {
                        // 座標の更新
                        {
                            var difference = this.networkPosition.Value - this.player.transform.localPosition;
                            if (difference.sqrMagnitude > this.warpPositionThreshold * this.warpPositionThreshold)
                            {
                                this.player.PostureController.Warp(this.networkPosition.Value);
                            }
                            else
                            {
                                var sqrMagnitude = difference.sqrMagnitude;
                                var threshold = this.playerActorCommonData.MoveSpeed * this.playerActorCommonData.MoveSpeed;
                                if (sqrMagnitude >= threshold)
                                {
                                    var direction = difference.normalized;
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.player, ActorEvents.RequestMove.Get(direction * this.playerActorCommonData.MoveSpeed * this.player.TimeController.Time.deltaTime));
                                }
                                else if (sqrMagnitude < threshold && sqrMagnitude > 0.01f)
                                {
                                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                                        .Publish(this.player, ActorEvents.RequestMove.Get(difference * this.playerActorCommonData.MoveSpeed * this.player.TimeController.Time.deltaTime));
                                }
                            }
                        }
                        // 回転の更新
                        {
                            var rotation = Quaternion.Lerp(
                                this.player.transform.localRotation,
                                Quaternion.Euler(0.0f, this.networkRotationY.Value, 0.0f),
                                this.playerActorCommonData.RotationSpeed * this.player.TimeController.Time.deltaTime
                                );
                            MessageBroker.GetPublisher<Actor, ActorEvents.RequestRotation>()
                                .Publish(this.player, ActorEvents.RequestRotation.Get(rotation));
                        }
                    })
                    .AddTo(ct);
            }
        }

        private void TrySubmitPosition()
        {
            var distance = (this.player.transform.localPosition - this.networkPosition.Value).sqrMagnitude;
            if (distance > this.sendPositionThreshold * this.sendPositionThreshold)
            {
                this.SubmitPositionServerRpc(this.player.transform.localPosition);
            }
        }

        private void TrySubmitRotationY()
        {
            var difference = this.networkRotationY.Value - this.player.transform.localRotation.eulerAngles.y;
            difference = difference < 0 ? -difference : difference;
            if (difference > this.sendRotationYThreshold)
            {
                this.SubmitRotationYServerRpc(this.player.transform.localRotation.eulerAngles.y);
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
    }
}