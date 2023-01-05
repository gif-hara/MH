
using System;
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
        public class SelectModeView
        {
            [SerializeField]
            private Button createLobbyButton;

            public IObservable<Unit> OnClickCreateLobby => this.createLobbyButton.OnClickAsObservable();
        }

        [SerializeField]
        private SelectModeView selectMode;

        public SelectModeView SelectMode => this.selectMode;
    }
}
