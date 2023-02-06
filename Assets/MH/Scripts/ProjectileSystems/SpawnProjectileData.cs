using UnityEngine;

namespace MH.ProjectileSystems
{
    /// <summary>
    /// <see cref="Projectile"/>の生成に必要なデータ
    /// </summary>
    [CreateAssetMenu(menuName = "MH/SpawnProjectileData")]
    public sealed class SpawnProjectileData : ScriptableObject
    {
        [SerializeField]
        private Projectile projectilePrefab;

        [SerializeField]
        private ProjectileData data;

        [SerializeField]
        private Vector3 positionOffset;

        [SerializeField]
        private Vector3 rotationOffset;

        public Projectile ProjectilePrefab => this.projectilePrefab;

        public ProjectileData Data => this.data;

        public Vector3 PositionOffset => this.positionOffset;

        public Vector3 RotationOffset => this.rotationOffset;
    }
}
