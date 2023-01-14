using UnityEngine;

namespace MH.SceneControllers
{
    /// <summary>
    /// バトルシーンを制御するクラス
    /// </summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        [SerializeField]
        private Actor playerPrefab;

        [SerializeField]
        private Actor enemyPrefab;

        [SerializeField]
        private BattleSceneDebugData debugData;

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

            var player = this.playerPrefab.Spawn(this.debugData.playerSpawnData.data, this.playerSpawnPoint);
            MessageBroker.GetPublisher<ActorEvents.SpawnedPlayer>()
                .Publish(ActorEvents.SpawnedPlayer.Get(player));

            this.enemyPrefab.Spawn(this.debugData.enemySpawnData.data, this.enemySpawnPoint);
        }
    }
}
