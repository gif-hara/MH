using MH.ActorControllers;
using UnityEngine;

namespace MH.ProjectileSystems
{
    /// <summary>
    /// 飛翔体の中枢となるクラス
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        private PrefabPool<Projectile> pool;

        private IProjectileController[] controllers;

        public void Spawn(ProjectileData data, Actor owner, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            this.pool = null;
#endif
            this.pool ??= new PrefabPool<Projectile>(this);
            var instance = this.pool.Get();
            var t = instance.transform;
            t.position = position;
            t.rotation = rotation;
            instance.controllers ??= instance.GetComponentsInChildren<IProjectileController>();
            foreach (var child in instance.controllers)
            {
                child.Setup(this, data, owner);
            }
        }
    }
}
