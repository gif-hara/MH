using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using MessagePipe;
using MH.NetworkSystems;
using MH.UISystems;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
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
            SearchLobby
        }

        [SerializeField]
        private LobbyUIView lobbyUIViewPrefab;

        private readonly List<Lobby> queryLobbies = new();

        private LobbyUIView lobbyUIView;

        private StateController<State> stateController;

        private async void Start()
        {
            await BootSystem.IsReady;

            lobbyUIView = UIManager.Open(lobbyUIViewPrefab);

            stateController = new StateController<State>(State.Invalid);
            stateController.Set(State.SelectMode, OnEnterSelectMode, null);
            stateController.Set(State.LobbyHost, OnEnterLobbyHost, null);
            stateController.Set(State.LobbyClient, OnEnterLobbyClient, null);
            stateController.Set(State.SearchLobby, OnEnterSearchLobby, null);
            stateController.ChangeRequest(State.SelectMode);
        }

        private void OnDestroy()
        {
            if (lobbyUIView != null)
                UIManager.Close(lobbyUIView);
        }

        private void OnEnterSelectMode(State previousState, DisposableBagBuilder scope)
        {
            lobbyUIView.SetActiveArea(lobbyUIView.SelectMode);

            lobbyUIView.SelectMode.OnClickCreateLobbyAsyncEnumerable()
                .Subscribe(async _ =>
                {
                    await MultiPlayManager.BeginAsHostAsync(4);
                    stateController.ChangeRequest(State.LobbyHost);
                })
                .AddTo(scope);

            lobbyUIView.SelectMode.OnClickSearchLobbyAsyncEnumerable()
                .Subscribe(async _ =>
                {
                    var query = await MultiPlayManager.QueryLobbies();
                    queryLobbies.Clear();
                    queryLobbies.AddRange(query.Results);
                    stateController.ChangeRequest(State.SearchLobby);
                })
                .AddTo(scope);
        }

        private void OnEnterLobbyHost(State previousState, DisposableBagBuilder scope)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
        }

        private void OnEnterLobbyClient(State previousState, DisposableBagBuilder scope)
        {
        }

        private void OnEnterSearchLobby(State previousState, DisposableBagBuilder scope)
        {
            lobbyUIView.SearchLobby.RemoveAllLobbyElement();
            foreach (var lobby in queryLobbies)
            {
                var element = lobbyUIView.SearchLobby.CreateLobbyElement();
                element.LobbyName = lobby.Name;
                element.OnClickAsyncEnumerable()
                    .Subscribe(async _ =>
                    {
                        await MultiPlayManager.BeginAsClientAsync(lobby.Id, lobby.Data["joinCode"].Value);
                        stateController.ChangeRequest(State.LobbyClient);
                    })
                    .AddTo(scope);
            }

            lobbyUIView.SetActiveArea(lobbyUIView.SearchLobby);
        }
    }
}
