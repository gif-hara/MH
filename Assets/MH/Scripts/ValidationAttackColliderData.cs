using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    [CreateAssetMenu(menuName = "MH/ValidationAttackColliderData")]
    public sealed class ValidationAttackColliderData : ScriptableObject
    {
        [SerializeField]
        private string colliderName;

        public string ColliderName => this.colliderName;
    }
}
