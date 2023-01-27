using UnityEngine;

namespace MH
{
    /// <summary>
    /// Poolされる想定のプレハブにアタッチされるクラス
    /// </summary>
    public sealed class PoolablePrefab : MonoBehaviour
    {
        [SerializeField]
        private float poolDelaySeconds;

        public float PoolDelaySeconds => this.poolDelaySeconds;
    }
}
