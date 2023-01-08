using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.NetworkSystems;
using MH.UISystems;
using UnityEngine;
using UnityEngine.Assertions;
using UniRx;
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

        private void OnEnterSelectMode(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.SelectMode.OnClickCreateLobby
                .Subscribe(async _ =>
                {
                    await MultiPlayManager.BeginAsHost(4);
                    this.stateController.ChangeRequest(State.LobbyHost);
                })
                .AddTo(scope);

            this.lobbyUIView.SelectMode.OnClickSearchLobby
                .Subscribe(async _ =>
                {
                    var query = await MultiPlayManager.QueryLobbies();
                    this.queryLobbies.Clear();
                    this.queryLobbies.AddRange(query.Results);
                    this.stateController.ChangeRequest(State.SearchLobby);
                })
                .AddTo(scope);
            
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.SelectMode);
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

        private void OnEnterSearchLobby(State previousState, DisposableBagBuilder scope)
        {
            this.lobbyUIView.SearchLobby.RemoveAllLobbyElement();
            foreach (var lobby in this.queryLobbies)
            {
                var element = this.lobbyUIView.SearchLobby.CreateLobbyElement();
                element.LobbyName = lobby.Name;
                element.OnClickButtonAsObservable()
                    .Subscribe(_ =>
                    {
                        Debug.Log("TODO");
                    })
                    .AddTo(scope);
            }
            
            this.lobbyUIView.SetActiveArea(this.lobbyUIView.SearchLobby);
        }
    }
}
