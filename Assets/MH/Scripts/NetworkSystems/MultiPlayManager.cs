using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.NetworkSystems
{
    /// <summary>
    ///
    /// </summary>
    public static class MultiPlayManager
    {
        /// <summary>
        /// 初期化済みであるか
        /// </summary>
        private static bool isInitialized;

        /// <summary>
        /// 通信中か返す
        /// </summary>
        public static bool IsConnecting => NetworkManager.Singleton.IsListening;

        public static async UniTask BeginAsHostAsync(int maxConnections, CreateLobbyOptions options = null)
        {
            try
            {
                await InitializeIfNeed();
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                options ??= new CreateLobbyOptions();
                options.Data ??= new Dictionary<string, DataObject>();
                options.Data.Add("joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode));
                await LobbyManager.CreateLobbyAsync(Guid.NewGuid().ToString(), 4, options);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                        );
                if (!NetworkManager.Singleton.StartHost())
                {
                    Debug.LogError("Failed Host.");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public static async UniTask BeginAsClientAsync(string lobbyId, string joinCode, JoinLobbyByIdOptions options = null)
        {
            try
            {
                await InitializeIfNeed();
                await LobbyManager.JoinLobbyAsync(lobbyId, options);
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(
                        joinAllocation.RelayServer.IpV4,
                        (ushort)joinAllocation.RelayServer.Port,
                        joinAllocation.AllocationIdBytes,
                        joinAllocation.Key,
                        joinAllocation.ConnectionData,
                        joinAllocation.HostConnectionData
                        );
                if (!NetworkManager.Singleton.StartClient())
                {
                    Debug.LogError("Failed Client.");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public static async UniTask<QueryResponse> QueryLobbies(QueryLobbiesOptions options = null)
        {
            try
            {
                await InitializeIfNeed();
                return await LobbyManager.QueryLobbiesAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public static IUniTaskAsyncEnumerable<ulong> OnClientConnectedAsyncEnumerable(CancellationToken ct)
        {
            return new ActionHandlerAsyncEnumerable<ulong>(
                x => NetworkManager.Singleton.OnClientConnectedCallback += x,
                x =>
                {
                    var networkManager = NetworkManager.Singleton;
                    if (networkManager != null)
                    {
                        networkManager.OnClientConnectedCallback -= x;
                    }
                },
                ct
                );
        }

        public static IUniTaskAsyncEnumerable<ulong> OnClientDisconnectAsyncEnumerable(CancellationToken ct)
        {
            return new ActionHandlerAsyncEnumerable<ulong>(
                x => NetworkManager.Singleton.OnClientDisconnectCallback += x,
                x =>
                {
                    var networkManager = NetworkManager.Singleton;
                    if (networkManager != null)
                    {
                        networkManager.OnClientDisconnectCallback -= x;
                    }
                },
                ct
                );
        }

        /// <summary>
        /// シングルプレイとして開始する
        /// </summary>
        public static void StartAsSinglePlay()
        {
            Assert.IsFalse(IsConnecting);
            NetworkManager.Singleton.StartHost();
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
