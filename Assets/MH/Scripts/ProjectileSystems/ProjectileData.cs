using System;
using MH.ProjectileSystems.Decorators;
using UnityEngine;

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

        public float durationSeconds;

        [SerializeReference, SubclassSelector(typeof(IProjectileDecorator))]
        public IProjectileDecorator[] decorators;
    }
}
