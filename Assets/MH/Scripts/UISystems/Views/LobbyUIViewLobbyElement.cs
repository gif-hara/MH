using Cysharp.Threading.Tasks;
using TMPro;
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

        public IUniTaskAsyncEnumerable<AsyncUnit> OnClickAsyncEnumerable() =>
            this.button.OnClickAsAsyncEnumerable();

        public string LobbyName
        {
            set => this.lobbyName.text = value;
        }
    }
}
