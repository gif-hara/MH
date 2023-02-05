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

        [SerializeField]
        private Slider staminaSlider;

        public Slider HitPointSlider => this.hitPointSlider;

        public Slider StaminaSlider => this.staminaSlider;
    }
}
