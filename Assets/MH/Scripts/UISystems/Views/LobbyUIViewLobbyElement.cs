using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MH.UISystems
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbyUIViewLobbyElement : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TextMeshProUGUI lobbyName;

        public IObservable<Unit> OnClickButtonAsObservable() => this.button.OnClickAsObservable();
        
        public string LobbyName
        {
            set => this.lobbyName.text = value;
        }
    }
}
