using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.ActorControllers;
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
        private Actor enemyPrefab;

        [SerializeField]
        private BattleSceneDebugData debugData;

        [SerializeField]
        private Transform playerSpawnPoint;

        [SerializeField]
        private Transform enemySpawnPoint;

        private async void Start()
        {
            await BootSystem.IsReady;

            var ct = this.GetCancellationTokenOnDestroy();

            MessageBroker.GetSubscriber<ActorEvents.AddedActor>()
                .Subscribe(x =>
                {
                    if (x.Actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
                    {
                        x.Actor.PostureController.Warp(this.playerSpawnPoint.position);
                    }
                })
                .AddTo(ct);

            foreach (var player in ActorManager.Players)
            {
                player.PostureController.Warp(this.playerSpawnPoint.position);
            }

            if (!MultiPlayManager.IsConnecting)
            {
                MultiPlayManager.StartAsSinglePlay();
            }

            this.enemyPrefab.Spawn(this.debugData.enemySpawnData.data, this.enemySpawnPoint);
        }
    }
}
