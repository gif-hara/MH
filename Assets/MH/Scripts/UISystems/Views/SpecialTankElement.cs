using UnityEngine;

namespace MH.UISystems
{
    /// <summary>
    /// </summary>
    public sealed class SpecialTankElement : MonoBehaviour
    {
        [SerializeField]
        private GameObject inactiveIcon;

        [SerializeField]
        private GameObject activeIcon;

        public void SetActiveIcon(bool isActive)
        {
            this.inactiveIcon.SetActive(!isActive);
            this.activeIcon.SetActive(isActive);
        }
    }
}
