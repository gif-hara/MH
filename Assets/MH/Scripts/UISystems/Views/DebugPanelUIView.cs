using TMPro;
using UnityEngine;

namespace MH.UISystems
{
    /// <summary>
    /// デバッグパネルの<see cref="UIView"/>
    /// </summary>
    public sealed class DebugPanelUIView : UIView
    {
        [SerializeField]
        private TextMeshProUGUI text;

        public string Text
        {
            get => this.text.text;
            set => this.text.text = value;
        }
    }
}
