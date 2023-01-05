using System;
using Cysharp.Threading.Tasks;
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
        public enum State
        {
            Invalid,
            SelectMode,
            LobbyHost,
        }
        
        [SerializeField]
        private LobbyUIView lobbyUIViewPrefab;

        private LobbyUIView lobbyUIView;

        private StateController<State> stateController;

        private async void Start()
        {
            await BootSystem.IsReady;

            this.lobbyUIView = UIManager.Open(this.lobbyUIViewPrefab);

            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.SelectMode, OnEnterSelectMode, null);
            this.stateController.Set(State.LobbyHost, OnEnterLobbyHost, null);
            this.stateController.ChangeRequest(State.SelectMode);
        }
        
        private void OnEnterSelectMode(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.SelectMode.OnClickCreateLobby
                .Subscribe(async _ =>
                {
                    await LobbyManager.CreateLobbyAsync();
                    this.stateController.ChangeRequest(State.LobbyHost);
                })
                .AddTo(scope);
            
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.SelectMode);
        }

        private void OnEnterLobbyHost(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.Lobby.OnClickDeleteLobbyAsObservable()
                .Subscribe(async _ =>
                {
                    await LobbyManager.DeleteLobbyAsync();
                    this.stateController.ChangeRequest(State.SelectMode);
                })
                .AddTo(scope);
            
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.Lobby);
            this.lobbyUIView.Lobby.SetActiveHostArea();
        }
    }
}
