using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="IActorController.Setup"/>のタイミングでエフェクトを生成するクラス
    /// </summary>
    public sealed class ActorSetupSpawnEffectPoint : MonoBehaviour, IActorController
    {
        [SerializeField]
        private List<PoolablePrefab> effectPrefabs;

        [SerializeField]
        private float waitSeconds;

        public async void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            using var cts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(this.waitSeconds), cancellationToken: cts.Token);

            var effectPrefab = this.effectPrefabs[Random.Range(0, this.effectPrefabs.Count)];
            MessageBroker.GetPublisher<PoolablePrefabEvents.RequestCreate>()
                .Publish(PoolablePrefabEvents.RequestCreate.Get(effectPrefab, this.transform.position, Quaternion.identity));

            cts.Cancel();
        }
    }
}
