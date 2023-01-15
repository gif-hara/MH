using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
        private PlayerInputController playerInputControllerPrefab;

        private readonly NetworkVariable<Vector3> position = new(Vector3.zero);

        private Actor player;

        public override void OnDestroy()
        {
            this.position.OnValueChanged -= OnPositionValueChanged;
        }

        public override void OnNetworkSpawn()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            this.player = this.actorPrefab.Spawn(this.actorSpawnData.data, Vector3.zero, Quaternion.identity);
            this.player.transform.SetParent(this.transform);
            if (this.IsOwner)
            {
                var inputController = Instantiate(this.playerInputControllerPrefab, this.transform);
                inputController.Attach(this.player);
                UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(1.0f))
                    .Subscribe(_ =>
                    {
                        this.SubmitTestServerRpc(this.player.transform.localPosition);
                    })
                    .AddTo(ct);
            }

            this.position.OnValueChanged += OnPositionValueChanged;
        }

        private void OnPositionValueChanged(Vector3 previousValue, Vector3 newValue)
        {
            this.player.transform.localPosition = newValue;
            Debug.Log($"OnPositionValueChanged[{this.OwnerClientId}] {newValue}");
        }

        [ServerRpc]
        private void SubmitTestServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
        {
            this.position.Value = newPosition;
            Debug.Log($"{this.OwnerClientId} {this.position.Value}");
        }
    }
}
