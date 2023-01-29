using UnityEngine;
using UnityEngine.UI;

namespace MH.UISystems
{
    /// <summary>
    /// バトルシーンの<see cref="UIView"/>
    /// </summary>
    public sealed class BattleUIView : UIView
    {
        [SerializeField]
        private Slider hitPointSlider;

        public Slider HitPointSlider => this.hitPointSlider;
    }
}
