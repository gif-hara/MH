using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.NetworkSystems
{
    /// <summary>
    /// ロビーを管理するクラス
    /// </summary>
    public static class LobbyManager
    {
        /// <summary>
        /// 現在参加しているロビー
        /// </summary>
        private static Lobby currentLobby;

        /// <summary>
        /// ロビー参加中のスコープ
        /// </summary>
        private static CancellationTokenSource inLobbyScope;

        /// <summary>
        /// 現在参加しているロビー
        /// </summary>
        public static Lobby Lobby => currentLobby;

        /// <summary>
        /// ロビーを作成する
        /// </summary>
        public static async UniTask CreateLobbyAsync(string lobbyName, int maxConnections, CreateLobbyOptions options = null)
        {
            try
            {
                currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxConnections, options);
                inLobbyScope?.Cancel();
                inLobbyScope?.Dispose();
                inLobbyScope = new CancellationTokenSource();
                BeginHeartbeatAsync(inLobbyScope.Token);

                // BeginGetLobbyAsync(inLobbyScope.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ロビーに参加する
        /// </summary>
        public static async UniTask JoinLobbyAsync(string lobbyId, JoinLobbyByIdOptions options = null)
        {
            try
            {
                currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);
                inLobbyScope?.Cancel();
                inLobbyScope?.Dispose();
                inLobbyScope = new CancellationTokenSource();

                // BeginGetLobbyAsync(inLobbyScope.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ロビーを削除する
        /// </summary>
        public static async UniTask DeleteLobbyAsync()
        {
            try
            {
                Assert.IsNotNull(currentLobby, $"{nameof(currentLobby)} != null");
                inLobbyScope.Cancel();
                inLobbyScope.Dispose();
                inLobbyScope = null;
                await Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                currentLobby = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// プレイヤーを削除する
        /// </summary>
        public static async UniTask RemovePlayerAsync(string playerId)
        {
            try
            {
                if (AuthenticationService.Instance.PlayerId == playerId)
                {
                    inLobbyScope?.Cancel();
                    inLobbyScope?.Dispose();
                    inLobbyScope = null;
                }
                await Lobbies.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    return;
                }

                Debug.LogException(e);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ハートビートを開始する
        /// </summary>
        public static async void BeginHeartbeatAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Lobbies.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                    await UniTask.Delay(TimeSpan.FromSeconds(6.0f), cancellationToken: token);
                }
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

        public static async UniTask WaitForDeleteLobby(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason != LobbyExceptionReason.LobbyNotFound)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ロビーの状態を取得する
        /// </summary>
        public static async void BeginGetLobbyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    currentLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);

                    // TODO:全てのプレイヤーがホストでない場合はロビーが削除されたと判断する


                    // 新規で追加されたプレイヤーを通知する
                    foreach (var newLobbyPlayer in currentLobby.Players)
                    {
                        var isNewPlayer = true;
                        foreach (var currentLobbyPlayer in currentLobby.Players)
                        {
                            if (newLobbyPlayer.Id == currentLobbyPlayer.Id)
                            {
                                isNewPlayer = false;
                                break;
                            }
                        }

                        if (isNewPlayer)
                        {
                            MessageBroker.GetPublisher<LobbyEvents.AddedPlayer>()
                                .Publish(LobbyEvents.AddedPlayer.Get(newLobbyPlayer));
                        }
                    }

                    // ロビーから削除されたプレイヤーを通知する
                    foreach (var currentLobbyPlayer in currentLobby.Players)
                    {
                        var isRemovePlayer = true;
                        foreach (var newLobbyPlayer in currentLobby.Players)
                        {
                            if (currentLobbyPlayer.Id == newLobbyPlayer.Id)
                            {
                                isRemovePlayer = false;
                                break;
                            }
                        }

                        if (isRemovePlayer)
                        {
                            MessageBroker.GetPublisher<LobbyEvents.RemovedPlayer>()
                                .Publish(LobbyEvents.RemovedPlayer.Get(currentLobbyPlayer));
                        }
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (LobbyServiceException e)
            {
                // TODO: ちゃんとハンドリングしたい
                Debug.LogWarning(e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ロビーの検索を行う
        /// </summary>
        public static async UniTask<QueryResponse> QueryLobbiesAsync(QueryLobbiesOptions options = null)
        {
            try
            {
                return await Lobbies.Instance.QueryLobbiesAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// ロビーに入っているか返す
        /// </summary>
        public static bool IsInLobby()
        {
            return currentLobby != null;
        }

#if UNITY_EDITOR
        public static void OnExitingPlayMode()
        {
            currentLobby = null;
            inLobbyScope?.Dispose();
            inLobbyScope = null;
        }
#endif
    }
}
