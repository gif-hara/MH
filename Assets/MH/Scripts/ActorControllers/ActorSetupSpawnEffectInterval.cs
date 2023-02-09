using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="IActorController.Setup"/>のタイミングでエフェクトを生成するクラス
    /// </summary>
    public sealed class ActorSetupSpawnEffectInterval : MonoBehaviour, IActorController
    {
        [SerializeField]
        private List<PoolablePrefab> effectPrefabs;

        [SerializeField]
        private float totalSeconds;

        [SerializeField]
        private float intervalSeconds;

        [SerializeField]
        private float size;

        public async void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            using var cts = new CancellationTokenSource();

            UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(this.intervalSeconds))
                .Subscribe(_ =>
                {
                    var effectPrefab = this.effectPrefabs[Random.Range(0, this.effectPrefabs.Count)];
                    var position = this.transform.position;
                    var randomPosition = Random.onUnitSphere * this.size;
                    randomPosition.y = randomPosition.y < 0.0f ? -randomPosition.y : randomPosition.y;
                    MessageBroker.GetPublisher<PoolablePrefabEvents.RequestCreate>()
                        .Publish(PoolablePrefabEvents.RequestCreate.Get(effectPrefab, position + randomPosition, Quaternion.identity));
                })
                .AddTo(cts.Token);

            await UniTask.Delay(TimeSpan.FromSeconds(this.totalSeconds), cancellationToken: cts.Token);

            cts.Cancel();
        }
    }
}
