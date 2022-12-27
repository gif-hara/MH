using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        [SerializeField]
        private Actor playerPrefab;

        [SerializeField]
        private PlayerInputController playerInputControllerPrefab;

        [SerializeField]
        private Transform playerSpawnPoint;

        private async void Start()
        {
            await BootSystem.IsReady;

            Instantiate(this.playerInputControllerPrefab, this.transform);

            var player = Instantiate(this.playerPrefab, this.playerSpawnPoint.position, this.playerSpawnPoint.rotation);
            MessageBroker.GetPublisher<ActorEvents.SpawnedPlayer>()
                .Publish(ActorEvents.SpawnedPlayer.Get(player));
        }
    }
}
