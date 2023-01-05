using System;
using MessagePipe;
using MH.NetworkSystems;
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
                    var lobby = await LobbyManager.CreateLobbyAsync();
                    uiView.SetActiveArea(uiView.Lobby);
                })
                .AddTo(bag);
            
            uiView.SetActiveArea(uiView.SelectMode);
        }
    }
}
