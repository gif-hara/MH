using UnityEngine;

namespace MH
{
    /// <summary>
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

        /// <summary>
        /// 攻撃が当たった際にスペシャルチャージが回復するか
        /// </summary>
        [SerializeField]
        private bool canRecoverySpecialCharge;

        public string ColliderName => this.colliderName;

        public PoolablePrefab HitEffectPrefab => this.hitEffectPrefab;

        public float HitStopTimeScale => this.hitStopTimeScale;

        public float HitStopDurationSeconds => this.hitStopDurationSeconds;

        public int Power => this.power;

        public bool CanRecoverySpecialCharge => this.canRecoverySpecialCharge;
    }
}
