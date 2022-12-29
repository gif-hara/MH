using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorAttachable
    {
        [SerializeField]
        private List<GameObject> colliders;
        
        private Actor actor;

        private Dictionary<string, GameObject> colliderDictionary;

        private IDisposable scope;

        public void Attach(Actor actor)
        {
            this.actor = actor;
            var t = this.transform;
            t.SetParent(this.actor.ModelController.BoneHolder.RightHand, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            this.colliderDictionary = this.colliders.ToDictionary(x => x.name);
            var bag = DisposableBag.CreateBuilder();
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.ValidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.colliderDictionary[x.ColliderName].SetActive(true);
                })
                .AddTo(bag);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.InvalidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.colliderDictionary[x.ColliderName].SetActive(false);
                })
                .AddTo(bag);

            this.scope = bag.Build();
        }

        private void OnDestroy()
        {
            this.scope?.Dispose();
        }
    }
}
