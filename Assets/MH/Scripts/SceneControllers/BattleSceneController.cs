using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using MH.ActorControllers;
using MH.NetworkSystems;
using MH.ProjectileSystems;
using MH.UISystems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private Transform playerWarpPoint;

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

            ProjectilePoolManager.BeginSystem();

            MessageBroker.GetSubscriber<BattleEvents.RequestJudgeResult>()
                .Subscribe(x =>
                {
                    MessageBroker.GetPublisher<BattleEvents.JudgedResult>()
                        .Publish(BattleEvents.JudgedResult.Get(x.Result));
                })
                .AddTo(ct);

            CancellationTokenSource startBattleCancellationTokenSource = null;

            void SetupPlayerActor(Actor player)
            {
                player.PostureController.Warp(this.playerSpawnPoint.position);
                MessageBroker.GetSubscriber<Actor, ActorEvents.ChangedIsReadyBattle>()
                    .Subscribe(player, x =>
                    {
                        if (!NetworkManager.Singleton.IsHost)
                        {
                            return;
                        }

                        if (x.IsReadyBattle)
                        {
                            foreach (var p in ActorManager.Players)
                            {
                                if (!p.NetworkController.NetworkBehaviour.NetworkIsReadyBattle)
                                {
                                    return;
                                }
                            }

                            startBattleCancellationTokenSource?.Cancel();
                            startBattleCancellationTokenSource?.Dispose();
                            startBattleCancellationTokenSource = new CancellationTokenSource();
                            StartBattleAsync(startBattleCancellationTokenSource.Token).Forget();
                        }
                        else
                        {
                            startBattleCancellationTokenSource?.Cancel();
                            startBattleCancellationTokenSource?.Dispose();
                            startBattleCancellationTokenSource = null;
                        }
                    })
                    .AddTo(ct);
            }

            async UniTaskVoid StartBattleAsync(CancellationToken token)
            {
                try
                {
                    Debug.Log("Wait StartBattle");
                    await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: token);
                    ActorManager.OwnerActor.NetworkController.NetworkBehaviour.RequestWarp(this.playerWarpPoint.position);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Cancel StartBattle");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }

            MessageBroker.GetSubscriber<ActorEvents.AddedActor>()
                .Subscribe(x =>
                {
                    if (x.Actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
                    {
                        SetupPlayerActor(x.Actor);
                    }
                })
                .AddTo(ct);

            foreach (var player in ActorManager.Players)
            {
                SetupPlayerActor(player);
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
                        EndBattle(5.0f).Forget();
                    }
                    else if (this.playerLoseCount >= ActorManager.Players.Count)
                    {
                        playerLoseUIView.PlayInAnimation();
                        EndBattle(5.0f).Forget();
                    }
                })
                .AddTo(ct);

            async UniTaskVoid EndBattle(float delaySeconds)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: ct);
                    if (NetworkManager.Singleton.IsHost)
                    {
                        if (MultiPlayManager.IsInLobby())
                        {
                            await MultiPlayManager.DeleteLobbyAsync();
                        }
                    }
                    else
                    {
                        await MultiPlayManager.WaitForDeleteLobby(ct);
                    }
                    UIManager.Close(uiView);
                    UIManager.Close(playerWinUIView);
                    UIManager.Close(playerLoseUIView);

                    // TODO: シーン管理ちゃんとしたら要らないかも
                    ProjectilePoolManager.EndSystem();

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

            this.GetAsyncUpdateTrigger()
                .Subscribe(async _ =>
                {
                    if (Keyboard.current.qKey.wasPressedThisFrame)
                    {
                        Debug.Log("Start Delete Lobby");
                        await MultiPlayManager.DeleteLobbyAsync();
                        UIManager.Close(uiView);
                        UIManager.Close(playerWinUIView);
                        UIManager.Close(playerLoseUIView);

                        SceneManager.LoadScene("Lobby");
                        Debug.Log("Complete Delete Lobby");
                    }
                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        Debug.Log("Start Remove MyPlayer");
                        await MultiPlayManager.RemoveMyPlayerAsync();
                        UIManager.Close(uiView);
                        UIManager.Close(playerWinUIView);
                        UIManager.Close(playerLoseUIView);

                        SceneManager.LoadScene("Lobby");
                        Debug.Log("Complete Remove MyPlayer");
                    }
                })
                .AddTo(ct);

            var isApplicationQuit = false;
            this.GetAsyncApplicationQuitTrigger()
                .Subscribe(_ =>
                {
                    isApplicationQuit = true;
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<BattleEvents.RequestBeginBattle>()
                .Subscribe(_ =>
                {
                    ActorManager.OwnerActor.PostureController.Warp(this.playerWarpPoint.position);
                })
                .AddTo(ct);

            // 終了処理
            {
                await this.OnDestroyAsync();

                if (!isApplicationQuit)
                {
                    ProjectilePoolManager.EndSystem();
                }
            }
        }
    }
}
