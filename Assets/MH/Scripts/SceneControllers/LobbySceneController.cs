using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using MessagePipe;
using MH.NetworkSystems;
using MH.UISystems;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

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
            LobbyClient,
            SearchLobby,
        }
        
        [SerializeField]
        private LobbyUIView lobbyUIViewPrefab;

        private LobbyUIView lobbyUIView;

        private StateController<State> stateController;

        private readonly List<Lobby> queryLobbies = new ();

        private async void Start()
        {
            await BootSystem.IsReady;

            this.lobbyUIView = UIManager.Open(this.lobbyUIViewPrefab);

            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.SelectMode, OnEnterSelectMode, null);
            this.stateController.Set(State.LobbyHost, OnEnterLobbyHost, null);
            this.stateController.Set(State.LobbyClient, OnEnterLobbyClient, null);
            this.stateController.Set(State.SearchLobby, OnEnterSearchLobby, null);
            this.stateController.ChangeRequest(State.SelectMode);
        }

        private void OnDestroy()
        {
            if (this.lobbyUIView != null)
            {
                UIManager.Close(this.lobbyUIView);
            }
        }

        private async void OnEnterSelectMode(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.SelectMode);

            this.lobbyUIView.SelectMode.OnClickCreateLobbyAsyncEnumerable()
                .Subscribe(async _ =>
                {
                    await MultiPlayManager.BeginAsHostAsync(4);
                    this.stateController.ChangeRequest(State.LobbyHost);
                })
                .AddTo(scope);

            this.lobbyUIView.SelectMode.OnClickSearchLobbyAsyncEnumerable()
                .Subscribe(async _ =>
                {
                    var query = await MultiPlayManager.QueryLobbies();
                    this.queryLobbies.Clear();
                    this.queryLobbies.AddRange(query.Results);
                    this.stateController.ChangeRequest(State.SearchLobby);
                })
                .AddTo(scope);
        }

        private void OnEnterLobbyHost(State previousState, DisposableBagBuilder scope)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
            // this.lobbyUIView.Lobby.OnClickDeleteLobbyAsObservable()
            //     .Subscribe(async _ =>
            //     {
            //         await LobbyManager.DeleteLobbyAsync();
            //         this.stateController.ChangeRequest(State.SelectMode);
            //     })
            //     .AddTo(scope);
            //
            // foreach (var player in LobbyManager.Lobby.Players)
            // {
            //     var playerElement = this.lobbyUIView.Lobby.CreatePlayerElement(player.Id);
            //     playerElement.PlayerName = player.Id;
            //     playerElement.SetActiveIsReady(false);
            // }
            // this.lobbyUIView.SetActiveArea(this.lobbyUIView.Lobby);
            // this.lobbyUIView.Lobby.SetActiveHostArea();
        }
        
        private void OnEnterLobbyClient(State previousState, DisposableBagBuilder scope)
        {
        }

        private void OnEnterSearchLobby(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.SearchLobby.RemoveAllLobbyElement();
            foreach (var lobby in this.queryLobbies)
            {
                var element = this.lobbyUIView.SearchLobby.CreateLobbyElement();
                element.LobbyName = lobby.Name;
                element.OnClickAsyncEnumerable()
                    .Subscribe(async _ =>
                    {
                        await MultiPlayManager.BeginAsClientAsync(lobby.Id, lobby.Data["joinCode"].Value);
                        this.stateController.ChangeRequest(State.LobbyClient);
                    })
                    .AddTo(scope);
            }
            
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.SearchLobby);
        }
    }
}
