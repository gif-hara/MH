using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="IActorController.Setup"/>のタイミングで<see cref="Actor"/>を死亡させるクラス
    /// </summary>
    public sealed class ActorSetupSpawnDestroyActor : MonoBehaviour, IActorController
    {
        [SerializeField]
        private float waitSeconds;

        public async void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            using var cts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(this.waitSeconds), cancellationToken: cts.Token);
            Destroy(actor.gameObject);

            cts.Cancel();
        }
    }
}
