using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorController
    {
        [SerializeField]
        private float hitStopTimeScale;

        [SerializeField]
        private float hitStopSeconds;

        [SerializeField]
        private List<GameObject> colliders;

        [SerializeField]
        private GameObject hitEffectPrefab;

        private readonly HashSet<Rigidbody> collidedRigidbodies = new();

        private Actor actor;

        private Dictionary<string, GameObject> colliderDictionary;

        private IDisposable scope;

        private void Awake()
        {
            foreach (var i in colliders)
            {
                i.SetActive(false);
            }

            colliderDictionary = colliders.ToDictionary(x => x.name);
        }

        private void OnDestroy()
        {
            scope?.Dispose();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (actor.gameObject == other.gameObject)
            {
                return;
            }

            var targetRigidbody = other.attachedRigidbody;
            if (this.collidedRigidbodies.Contains(targetRigidbody))
            {
                return;
            }

            this.collidedRigidbodies.Add(targetRigidbody);

            var hitData = new HitData
            {
                position = other.ClosestPoint(transform.position),
                rotation = transform.rotation
            };

            Instantiate(hitEffectPrefab, hitData.position, hitData.rotation);

            MessageBroker.GetPublisher<ActorEvents.HitAttack>()
                .Publish(ActorEvents.HitAttack.Get(hitData));

            actor.TimeController.BeginHitStop(hitStopTimeScale, hitStopSeconds);
        }

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            var t = transform;
            t.SetParent(this.actor.ModelController.BoneHolder.RightHand, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            var bag = DisposableBag.CreateBuilder();

            MessageBroker.GetSubscriber<Actor, ActorEvents.ValidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.collidedRigidbodies.Clear();
                    this.colliderDictionary[x.ColliderName].SetActive(true);
                })
                .AddTo(bag);

            MessageBroker.GetSubscriber<Actor, ActorEvents.InvalidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.colliderDictionary[x.ColliderName].SetActive(false);
                })
                .AddTo(bag);

            scope = bag.Build();
        }
    }
}
