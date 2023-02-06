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
        private string spawnLocatorName;

        public Projectile ProjectilePrefab => this.projectilePrefab;

        public ProjectileData Data => this.data;

        public string SpawnLocatorName => this.spawnLocatorName;
    }
}
