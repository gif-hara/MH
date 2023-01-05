using System;
using MessagePipe;
using MH.UISystems;
using UnityEngine;
using UnityEngine.Assertions;
using UniRx;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbySceneController : MonoBehaviour
    {
        [SerializeField]
        private LobbyUIView lobbyUIViewPrefab;

        private IDisposable scope;
        
        private async void Start()
        {
            await BootSystem.IsReady;

            var bag = DisposableBag.CreateBuilder();
            var uiView = UIManager.Open(this.lobbyUIViewPrefab);
            
            uiView.SelectMode.OnClickCreateLobby
                .Subscribe(async _ =>
                {
                    await LobbyManager.CreateLobbyAsync();
                    
                    Debug.Log("Create Lobby");
                })
                .AddTo(bag);
        }
    }
}
