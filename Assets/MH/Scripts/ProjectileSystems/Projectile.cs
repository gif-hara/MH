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
        private IProjectileController[] controllers;

        private CancellationTokenSource onReleasePoolScope;

        public CancellationToken OnReleaseToken => this.onReleasePoolScope.Token;

        public Time Time { get; } = new(TimeManager.Game);

        public void Spawn(
            ProjectileData data,
            Actor owner,
            Vector3 position,
            Quaternion rotation
        )
        {
            var pool = ProjectilePoolManager.GetPool(this);
            var instance = pool.Get();
            instance.Setup(data, owner, position, rotation, pool).Forget();
        }

        private async UniTaskVoid Setup(
            ProjectileData data,
            Actor owner,
            Vector3 position,
            Quaternion rotation,
            PrefabPool<Projectile> pool
        )
        {
            var t = this.transform;
            t.position = position;
            t.rotation = rotation;
            using (this.onReleasePoolScope = new CancellationTokenSource())
            {
                this.controllers ??= this.GetComponentsInChildren<IProjectileController>();
                foreach (var child in this.controllers)
                {
                    child.Setup(this, data, owner);
                }

                foreach (var decorator in data.decorators)
                {
                    decorator.Decorate(this);
                }

                this.GetAsyncTriggerEnterTrigger()
                    .Subscribe(async _ =>
                    {
                        // 1フレーム待たないとダメージ処理されない・・・
                        await UniTask.NextFrame(this.OnReleaseToken);
                        pool.Release(this);
                        this.onReleasePoolScope.Cancel();
                        this.onReleasePoolScope.Dispose();
                    })
                    .AddTo(this.OnReleaseToken);

                await UniTask.Delay(TimeSpan.FromSeconds(data.durationSeconds), cancellationToken: this.OnReleaseToken);

                pool.Release(this);
            }
        }
    }
}
