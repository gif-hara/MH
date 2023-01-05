using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MH.UISystems
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbyUIView : UIView
    {
        [Serializable]
        public class Area
        {
            [SerializeField]
            private GameObject areaRoot;

            public void SetActiveArea(Area area)
            {
                this.areaRoot.SetActive(this.areaRoot == area.areaRoot);
            }
        }
        
        [Serializable]
        public class SelectModeArea : Area
        {
            [SerializeField]
            private Button createLobbyButton;

            [SerializeField]
            private Button searchLobbyButton;

            public IObservable<Unit> OnClickCreateLobby => this.createLobbyButton.OnClickAsObservable();

            public IObservable<Unit> OnClickSearchLobby => this.searchLobbyButton.OnClickAsObservable();
        }

        [Serializable]
        public class LobbyArea : Area
        {
            [SerializeField]
            private RectTransform playerListRoot;

            [SerializeField]
            private GameObject hostArea;

            [SerializeField]
            private GameObject clientArea;

            [SerializeField]
            private GameObject inPreparationArea;

            [SerializeField]
            private GameObject readyArea;

            [SerializeField]
            private Button startGameButton;

            [SerializeField]
            private Button deleteLobbyButton;
            
            [SerializeField]
            private Button gotoReadyButton;

            [SerializeField]
            private Button gotoInPreparationButton;

            [SerializeField]
            private LobbyUIViewPlayerElement playerElementPrefab;

            private Dictionary<string, LobbyUIViewPlayerElement> playerElements = new();

            public LobbyUIViewPlayerElement CreatePlayerElement(string id)
            {
                var result = Instantiate(this.playerElementPrefab, this.playerListRoot);
                this.playerElements.Add(id, result);

                return result;
            }

            public void RemovePlayerElement(string id)
            {
                if (this.playerElements.TryGetValue(id, out var playerElement))
                {
                    Destroy(playerElement.gameObject);
                    this.playerElements.Remove(id);
                }
            }

            public void RemovePlayerElementAll()
            {
                foreach (var pair in this.playerElements)
                {
                    Destroy(pair.Value.gameObject);
                }
                
                this.playerElements.Clear();
            }

            public void SetActiveHostArea()
            {
                this.hostArea.SetActive(true);
                this.clientArea.SetActive(false);
            }

            public void SetActiveClientArea()
            {
                this.hostArea.SetActive(false);
                this.clientArea.SetActive(true);
            }

            public void SetActiveInPreparationArea()
            {
                this.inPreparationArea.SetActive(true);
                this.readyArea.SetActive(false);
            }

            public void SetActiveReadyArea()
            {
                this.inPreparationArea.SetActive(false);
                this.readyArea.SetActive(true);
            }

            public bool StartGameButtonInteractable
            {
                set => this.startGameButton.interactable = value;
            }

            public IObservable<Unit> OnClickStartGameAsObservable() => this.startGameButton.OnClickAsObservable();

            public IObservable<Unit> OnClickDeleteLobbyAsObservable() => this.deleteLobbyButton.OnClickAsObservable();

            public IObservable<Unit> OnClickGotoInPreparationAsObservable() => this.gotoInPreparationButton.OnClickAsObservable();

            public IObservable<Unit> OnClickGotoReadyAsObservable() => this.gotoReadyButton.OnClickAsObservable();
        }


        [SerializeField]
        private SelectModeArea selectMode;

        [SerializeField]
        private LobbyArea lobby;

        public SelectModeArea SelectMode => this.selectMode;

        public LobbyArea Lobby => this.lobby;

        public void SetActiveArea(Area area)
        {
            this.selectMode.SetActiveArea(area);
            this.lobby.SetActiveArea(area);
        }
    }
}
