using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ProjectileSystems
{
    /// <summary>
    /// <see cref="Projectile"/>のPoolを管理するクラス
    /// </summary>
    public static class ProjectilePoolManager
    {
        private static PrefabPoolDictionary<Projectile> poolDictionary;

        public static void BeginSystem()
        {
            Assert.IsNull(poolDictionary, $"{nameof(poolDictionary)} == null");
            poolDictionary = new PrefabPoolDictionary<Projectile>();
            Debug.Log("BeginSystem");
        }

        public static void EndSystem()
        {
            poolDictionary?.Dispose();
            poolDictionary = null;
        }

        public static PrefabPool<Projectile> GetPool(Projectile prefab)
        {
            Assert.IsNotNull(poolDictionary, $"{nameof(poolDictionary)} != null");
            return poolDictionary.Get(prefab);
        }

#if UNITY_EDITOR
        public static void OnEnteredPlayMode()
        {
            poolDictionary?.Dispose();
            poolDictionary = null;
            Debug.Log("OnEnteredPlayMode");
        }
#endif
    }
}
