using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
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

        private CancellationTokenSource onReleasePoolScope;

        public CancellationToken OnReleaseToken => this.onReleasePoolScope.Token;

        public Time Time { get; } = new(TimeManager.Game);

        public async UniTaskVoid Spawn(ProjectileData data, Actor owner, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            this.pool = null;
#endif
            this.pool ??= new PrefabPool<Projectile>(this);
            var instance = this.pool.Get();
            var t = instance.transform;
            t.position = position;
            t.rotation = rotation;
            using (instance.onReleasePoolScope = new CancellationTokenSource())
            {
                instance.controllers ??= instance.GetComponentsInChildren<IProjectileController>();
                foreach (var child in instance.controllers)
                {
                    child.Setup(this, data, owner);
                }

                foreach (var decorator in data.decorators)
                {
                    decorator.Decorate(instance);
                }

                instance.GetAsyncTriggerEnterTrigger()
                    .Subscribe(async _ =>
                    {
                        // 1フレーム待たないとダメージ処理されない・・・
                        await UniTask.NextFrame(instance.OnReleaseToken);
                        this.pool.Release(instance);
                        instance.onReleasePoolScope.Cancel();
                        instance.onReleasePoolScope.Dispose();
                    })
                    .AddTo(instance.OnReleaseToken);

                await UniTask.Delay(TimeSpan.FromSeconds(data.durationSeconds), cancellationToken: instance.OnReleaseToken);

                this.pool.Release(instance);
            }
        }
    }
}
