using System.Collections.Generic;
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

        [SerializeField]
        private Slider specialChargeSlider;

        [SerializeField]
        private Transform specialTankParent;

        [SerializeField]
        private SpecialTankElement specialTankElementPrefab;

        public List<SpecialTankElement> SpecialTankElements { get; } = new();

        public Slider HitPointSlider => this.hitPointSlider;

        public Slider StaminaSlider => this.staminaSlider;

        public Slider SpecialChargeSlider => this.specialChargeSlider;

        public void CreateSpecialTankElements(int count)
        {
            foreach (var specialTankElement in this.SpecialTankElements)
            {
                Destroy(specialTankElement.gameObject);
            }
            this.SpecialTankElements.Clear();

            for (var i = 0; i < count; i++)
            {
                var element = Instantiate(this.specialTankElementPrefab, this.specialTankParent);
                element.SetActiveIcon(false);
                this.SpecialTankElements.Add(element);
            }
        }
    }
}
