using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public static class LobbyManager
    {
        public static async UniTask CreateLobbyAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await Lobbies.Instance.CreateLobbyAsync(Guid.NewGuid().ToString(), 4);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
