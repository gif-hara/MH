using System;

namespace MH.ProjectileSystems
{
    /// <summary>
    /// <see cref="Projectile"/>を構成するのに必要なデータ
    /// </summary>
    [Serializable]
    public struct ProjectileData
    {
        public int motionPower;

        public PoolablePrefab hitEffectPrefab;
    }
}
