using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
        public static async UniTask BeginAsHost(int maxConnections)
        {
            try
            {
                await LobbyManager.CreateLobbyAsync();
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                        );
                var isSuccess = NetworkManager.Singleton.StartHost();
                if (!isSuccess)
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

        public static async UniTask<QueryResponse> QueryLobbies(QueryLobbiesOptions options = null)
        {
            try
            {
                return await LobbyManager.QueryLobbiesAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
