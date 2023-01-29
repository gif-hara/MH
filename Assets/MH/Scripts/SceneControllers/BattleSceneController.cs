using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MessagePipe;
using MH.ActorControllers;
using MH.NetworkSystems;
using MH.UISystems;
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

        [SerializeField]
        private BattleUIView battleUIView;

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

            var uiView = UIManager.Open(this.battleUIView);
            var ownerActor = ActorManager.OwnerActor;
            var s = ownerActor.StatusController;
            ownerActor.StatusController.HitPoint
                .Subscribe(x =>
                {
                    uiView.HitPointSlider.value = (float)s.HitPoint.Value / s.HitPointMax.Value;
                })
                .AddTo(ct);
            ownerActor.StatusController.HitPointMax
                .Subscribe(x =>
                {
                    uiView.HitPointSlider.value = (float)s.HitPoint.Value / s.HitPointMax.Value;
                })
                .AddTo(ct);

            this.enemyPrefab.Spawn(this.debugData.enemySpawnData.data, this.enemySpawnPoint);
        }
    }
}
