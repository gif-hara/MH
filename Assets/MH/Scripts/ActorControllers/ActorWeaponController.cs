using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorController
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private List<GameObject> colliders;

        private PoolablePrefab hitEffectPrefab;

        private float hitStopTimeScale;

        private float hitStopDurationSeconds;

        private readonly HashSet<Rigidbody> collidedRigidbodies = new();

        private Dictionary<string, GameObject> colliderDictionary;

        private void Awake()
        {
            foreach (var i in colliders)
            {
                i.SetActive(false);
            }

            colliderDictionary = colliders.ToDictionary(x => x.name);
        }

        private void OnTriggerStay(Collider other)
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

            MessageBroker.GetPublisher<PoolablePrefabEvents.RequestCreate>()
                .Publish(PoolablePrefabEvents.RequestCreate.Get(this.hitEffectPrefab, hitData.position, hitData.rotation));

            MessageBroker.GetPublisher<ActorEvents.HitAttack>()
                .Publish(ActorEvents.HitAttack.Get(hitData));

            if (this.hitStopDurationSeconds > 0.0f)
            {
                actor.TimeController.BeginHitStop(hitStopTimeScale, hitStopDurationSeconds);
            }
        }

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            var ct = this.actor.GetCancellationTokenOnDestroy();

            MessageBroker.GetSubscriber<Actor, ActorEvents.ValidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.collidedRigidbodies.Clear();
                    this.colliderDictionary[x.Data.ColliderName].SetActive(true);
                    this.hitEffectPrefab = x.Data.HitEffectPrefab;
                    this.hitStopTimeScale = x.Data.HitStopTimeScale;
                    this.hitStopDurationSeconds = x.Data.HitStopDurationSeconds;
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.InvalidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.colliderDictionary[x.ColliderName].SetActive(false);
                })
                .AddTo(ct);
        }
    }
}
