using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.ActorControllers;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="ActorEvents"/>をフックしてパーティクルの再生フラグを設定するクラス
    /// </summary>
    public abstract class SetParticlePlayFromActorEvent<TValue> : MonoBehaviour
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
                        if (targetData.isPlay)
                        {
                            targetData.target.Play();
                        }
                        else
                        {
                            targetData.target.Stop();
                        }
                    }
                })
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        [Serializable]
        public class TargetData
        {
            public ParticleSystem target;

            public bool isPlay;
        }
    }
}
