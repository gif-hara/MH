using System;
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
    /// 
    /// </summary>
    public static class LobbyManager
    {
        private static bool isInitialized = false;

        private static Lobby currentLobby;

        private static CancellationTokenSource heartbeatScope;
        
        public static async UniTask<Lobby> CreateLobbyAsync()
        {
            try
            {
                await InitializeIfNeed();
                currentLobby = await Lobbies.Instance.CreateLobbyAsync(Guid.NewGuid().ToString(), 4);
                heartbeatScope = new CancellationTokenSource();
                StartHeartbeat(heartbeatScope.Token);

                return currentLobby;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public static async UniTask DeleteLobbyAsync()
        {
            try
            {
                await InitializeIfNeed();
                Assert.IsNotNull(currentLobby);
                await Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                Assert.IsNotNull(heartbeatScope);
                heartbeatScope.Cancel();
                heartbeatScope.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public static async void StartHeartbeat(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Debug.Log($"Heart Beat {currentLobby.Id}");
                    await Lobbies.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                    await UniTask.Delay(TimeSpan.FromSeconds(6.0f), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

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
