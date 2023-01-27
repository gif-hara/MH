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

        [SerializeField]
        private PoolablePrefab hitEffectPrefab;

        public string ColliderName => this.colliderName;

        public PoolablePrefab HitEffectPrefab => this.hitEffectPrefab;
    }
}
