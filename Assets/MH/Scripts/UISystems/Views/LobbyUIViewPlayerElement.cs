using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.UISystems
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbyUIViewPlayerElement : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerName;

        [SerializeField]
        private GameObject isReadyObject;

        public string PlayerName
        {
            set => this.playerName.text = value;
        }

        public void SetActiveIsReady(bool isActive)
        {
            this.isReadyObject.SetActive(isActive);
        }
    }
}
