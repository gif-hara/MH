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
    public abstract class SetTrailEmittingFromActorEvent<TValue> : MonoBehaviour, IActorAttachable
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private List<TargetData> targetDataList;

        [Serializable]
        public class TargetData
        {
            public TrailRenderer target;

            public bool emitting;
        }
        
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
        
        public void Attach(Actor actor)
        {
            this.actor = actor;
        }
    }
}