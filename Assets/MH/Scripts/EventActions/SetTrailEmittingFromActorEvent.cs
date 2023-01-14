using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="ActorEvents"/>をフックしてトレイルの再生フラグを設定するクラス
    /// </summary>
    public abstract class SetTrailEmittingFromActorEvent<TValue> : MonoBehaviour, IActorController
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private List<TargetData> targetDataList;

        private void Start()
        {
            MessageBroker.GetSubscriber<Actor, TValue>()
                .Subscribe(this.actor, _ =>
                {
                    foreach (var targetData in this.targetDataList)
                    {
                        targetData.target.emitting = targetData.emitting;
                    }
                })
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void Setup(Actor actor, ActorSpawnData spawnData)
        {
            this.actor = actor;
        }

        [Serializable]
        public class TargetData
        {
            public TrailRenderer target;

            public bool emitting;
        }
    }
}
