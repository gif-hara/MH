using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MH.NetworkSystems;
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

            if (!MultiPlayManager.IsConnecting)
            {
                MultiPlayManager.StartAsSinglePlay();
            }

            Instantiate(this.playerInputControllerPrefab, this.transform);

            var ct = this.GetCancellationTokenOnDestroy();

            MultiPlayManager.OnClientConnectedAsyncEnumerable(ct)
                .Subscribe(x =>
                {
                    Debug.Log($"OnClientConnected {x}");
                })
                .AddTo(ct);

            MultiPlayManager.OnClientDisconnectAsyncEnumerable(ct)
                .Subscribe(x =>
                {
                    Debug.Log($"OnClientDisconnect {x}");
                })
                .AddTo(ct);

            var player = this.playerPrefab.Spawn(this.debugData.playerSpawnData.data, this.playerSpawnPoint);
            MessageBroker.GetPublisher<ActorEvents.SpawnedPlayer>()
                .Publish(ActorEvents.SpawnedPlayer.Get(player));

            this.enemyPrefab.Spawn(this.debugData.enemySpawnData.data, this.enemySpawnPoint);
        }
    }
}
