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
        private Actor enemyPrefab;

        [SerializeField]
        private ActorSpawnData playerSpawnData;

        [SerializeField]
        private PlayerInputController playerInputControllerPrefab;

        [SerializeField]
        private Transform playerSpawnPoint;

        [SerializeField]
        private Transform enemySpawnPoint;

        private async void Start()
        {
            await BootSystem.IsReady;

            Instantiate(this.playerInputControllerPrefab, this.transform);

            var player = this.playerPrefab.Spawn(this.playerSpawnData, this.playerSpawnPoint);
            MessageBroker.GetPublisher<ActorEvents.SpawnedPlayer>()
                .Publish(ActorEvents.SpawnedPlayer.Get(player));

            this.enemyPrefab.Spawn(this.playerSpawnData, this.enemySpawnPoint);
        }
    }
}
