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

        public override void OnNetworkSpawn()
        {
            var player = this.actorPrefab.Spawn(this.actorSpawnData.data, Vector3.zero, Quaternion.identity);
            player.transform.SetParent(this.transform);
            if (this.IsOwner)
            {
                var inputController = Instantiate(this.playerInputControllerPrefab, this.transform);
                inputController.Attach(player);
            }
        }
    }
}
