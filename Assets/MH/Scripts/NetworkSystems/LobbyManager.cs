using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
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
        /// 初期化済みであるか
        /// </summary>
        private static bool isInitialized = false;

        /// <summary>
        /// 現在参加しているロビー
        /// </summary>
        private static Lobby currentLobby;

        /// <summary>
        /// ロビー参加中のスコープ
        /// </summary>
        private static CancellationTokenSource lobbyScope;

        /// <summary>
        /// 現在参加しているロビー
        /// </summary>
        public static Lobby Lobby => currentLobby;

        /// <summary>
        /// ロビーを作成する
        /// </summary>
        public static async UniTask CreateLobbyAsync()
        {
            try
            {
                await InitializeIfNeed();
                currentLobby = await Lobbies.Instance.CreateLobbyAsync(Guid.NewGuid().ToString(), 4);
                lobbyScope = new CancellationTokenSource();
                BeginHeartbeatAsync(lobbyScope.Token);
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
                await InitializeIfNeed();
                Assert.IsNotNull(currentLobby);
                await Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                currentLobby = null;
                Assert.IsNotNull(lobbyScope);
                lobbyScope.Cancel();
                lobbyScope.Dispose();
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

        /// <summary>
        /// ロビーの状態を取得する
        /// </summary>
        public static async void BeginGetLobbyAsync(CancellationToken token)
        {
            try
            {
                var newLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);
                
                // 新規で追加されたプレイヤーを通知する
                foreach (var newLobbyPlayer in newLobby.Players)
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
                    foreach (var newLobbyPlayer in newLobby.Players)
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

        /// <summary>
        /// ロビーの検索を行う
        /// </summary>
        public static async UniTask<QueryResponse> QueryLobbiesAsync(QueryLobbiesOptions options = null)
        {
            try
            {
                await InitializeIfNeed();
                return await Lobbies.Instance.QueryLobbiesAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// 必要であれば初期化を行う
        /// </summary>
        private static async UniTask InitializeIfNeed()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
