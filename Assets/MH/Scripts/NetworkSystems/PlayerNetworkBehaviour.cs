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
        private ActorSpawnData actorSpawnData;

        [SerializeField]
        private Actor actorPrefab;

        public override void OnNetworkSpawn()
        {
            Debug.Log("OnNetworkSpawn");
        }
    }
}
