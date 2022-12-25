using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="ActorEvents"/>をフックして<see cref="GameObject"/>のアクティブフラグを設定するクラス
    /// </summary>
    public abstract class SetActiveFromActorEvent<TValue> : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private List<TargetData> targetDataList;

        [Serializable]
        public class TargetData
        {
            public GameObject target;

            public bool isActive;
        }
        
        private void Start()
        {
            MessageBroker.GetSubscriber<Actor, TValue>()
                .Subscribe(this.actor, _ =>
                {
                    foreach (var targetData in this.targetDataList)
                    {
                        targetData.target.SetActive(targetData.isActive);
                    }
                })
                .AddTo(this.GetCancellationTokenOnDestroy());
        }
    }
}
