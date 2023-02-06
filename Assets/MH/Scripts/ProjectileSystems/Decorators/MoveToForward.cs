using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace MH.ProjectileSystems.Decorators
{
    /// <summary>
    /// 前方へ移動する<see cref="IProjectileDecorator"/>
    /// </summary>
    [Serializable]
    public sealed class MoveToForward : IProjectileDecorator
    {
        [SerializeField]
        private float speed;

        public UniTaskVoid Decorate(Projectile projectile)
        {
            projectile.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    var t = projectile.transform;
                    t.position += t.forward * this.speed * projectile.Time.deltaTime;
                })
                .AddTo(projectile.OnReleaseToken);

            return new UniTaskVoid();
        }
    }
}
