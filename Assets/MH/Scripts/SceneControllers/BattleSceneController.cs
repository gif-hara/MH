using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MessagePipe;
using MH.ActorControllers;
using MH.NetworkSystems;
using MH.UISystems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MH.SceneControllers
{
    /// <summary>
    /// バトルシーンを制御するクラス
    /// </summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        [SerializeField]
        private EnemyNetworkBehaviour enemyPrefab;

        [SerializeField]
        private BattleSceneDebugData debugData;

        [SerializeField]
        private Transform playerSpawnPoint;

        [SerializeField]
        private Transform enemySpawnPoint;

        [SerializeField]
        private BattleUIView battleUIView;

        [SerializeField]
        private BattleJudgementUIView battlePlayerWinUIView;

        [SerializeField]
        private BattleJudgementUIView battlePlayerLoseUIView;

        private int playerWinCount;

        private int playerLoseCount;

        private async void Start()
        {
            await BootSystem.IsReady;

            var ct = this.GetCancellationTokenOnDestroy();

            MessageBroker.GetSubscriber<BattleEvents.RequestJudgeResult>()
                .Subscribe(x =>
                {
                    MessageBroker.GetPublisher<BattleEvents.JudgedResult>()
                        .Publish(BattleEvents.JudgedResult.Get(x.Result));
                })
                .AddTo(ct);

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
            s.HitPoint
                .Subscribe(x =>
                {
                    uiView.HitPointSlider.value = s.HitPoint.Value / s.HitPointMax.Value;
                })
                .AddTo(ct);
            s.HitPointMax
                .Subscribe(x =>
                {
                    uiView.HitPointSlider.value = s.HitPoint.Value / s.HitPointMax.Value;
                })
                .AddTo(ct);

            s.Stamina
                .Subscribe(x =>
                {
                    uiView.StaminaSlider.value = s.Stamina.Value / s.StaminaMax.Value;
                })
                .AddTo(ct);

            s.StaminaMax
                .Subscribe(x =>
                {
                    uiView.StaminaSlider.value = s.Stamina.Value / s.StaminaMax.Value;
                })
                .AddTo(ct);

            s.SpecialGauge
                .Subscribe(x =>
                {
                    uiView.SpecialChargeSlider.value = (float)x / Define.SpecialGaugeMax;
                })
                .AddTo(ct);

            uiView.CreateSpecialTankElements(Define.SpecialTankMax);
            s.SpecialTank
                .Subscribe(x =>
                {
                    for (var i = 0; i < uiView.SpecialTankElements.Count; i++)
                    {
                        uiView.SpecialTankElements[i].SetActiveIcon(x >= i + 1);
                    }
                })
                .AddTo(ct);

            var playerWinUIView = UIManager.Open(this.battlePlayerWinUIView);
            var playerLoseUIView = UIManager.Open(this.battlePlayerLoseUIView);
            MessageBroker.GetSubscriber<BattleEvents.JudgedResult>()
                .Subscribe(x =>
                {
                    if (x.Result == Define.BattleResult.PlayerWin)
                    {
                        this.playerWinCount++;
                    }
                    else
                    {
                        this.playerLoseCount++;
                    }

                    if (this.playerWinCount >= ActorManager.Enemies.Count)
                    {
                        playerWinUIView.PlayInAnimation();
                        ToLobbyAsync(8.0f).Forget();
                    }
                    else if (this.playerLoseCount >= ActorManager.Players.Count)
                    {
                        playerLoseUIView.PlayInAnimation();
                        ToLobbyAsync(5.0f).Forget();
                    }
                })
                .AddTo(ct);

            async UniTaskVoid ToLobbyAsync(float delaySeconds)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: ct);
                    UIManager.Close(uiView);
                    UIManager.Close(playerWinUIView);
                    UIManager.Close(playerLoseUIView);

                    SceneManager.LoadScene("Lobby");
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }

            if (NetworkManager.Singleton.IsHost)
            {
                Instantiate(this.enemyPrefab, this.enemySpawnPoint.position, this.enemySpawnPoint.rotation);
            }
        }
    }
}
