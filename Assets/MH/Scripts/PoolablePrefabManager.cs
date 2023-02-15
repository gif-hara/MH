using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="PoolablePrefab"/>を管理するクラス
    /// </summary>
    public sealed class PoolablePrefabManager : MonoBehaviour
    {
        private readonly PrefabPoolDictionary<PoolablePrefab> prefabPoolDictionary = new();

        private void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<PoolablePrefabEvents.RequestCreate>()
                .Subscribe(x =>
                {
                    this.Create(x.Prefab, x.Position, x.Rotation).Forget();
                })
                .AddTo(ct);
        }

        private async UniTaskVoid Create(PoolablePrefab prefab, Vector3 position, Quaternion rotation)
        {
            var pool = this.prefabPoolDictionary.Get(prefab);
            var instance = pool.Get();
            var t = instance.transform;
            t.position = position;
            t.rotation = rotation;

            await UniTask.Delay(TimeSpan.FromSeconds(prefab.PoolDelaySeconds), cancellationToken: this.GetCancellationTokenOnDestroy());

            pool.Release(instance);
        }
    }
}
