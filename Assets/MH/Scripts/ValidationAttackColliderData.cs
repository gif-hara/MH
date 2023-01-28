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

        [SerializeField]
        private float hitStopTimeScale;

        [SerializeField]
        private float hitStopDurationSeconds;

        [SerializeField]
        private int power;

        public string ColliderName => this.colliderName;

        public PoolablePrefab HitEffectPrefab => this.hitEffectPrefab;

        public float HitStopTimeScale => this.hitStopTimeScale;

        public float HitStopDurationSeconds => this.hitStopDurationSeconds;

        public int Power => this.power;
    }
}
