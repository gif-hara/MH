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
        private int weaponPower;

        [SerializeField]
        private List<GameObject> colliders;

        private PoolablePrefab hitEffectPrefab;

        private float hitStopTimeScale;

        private float hitStopDurationSeconds;

        private int motionPower;

        private readonly HashSet<Actor> collidedRigidbodies = new();

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
            if (this.actor.gameObject == other.gameObject)
            {
                return;
            }

            var targetActor = other.GetComponentInParent<Actor>();

            if (this.collidedRigidbodies.Contains(targetActor))
            {
                return;
            }

            this.collidedRigidbodies.Add(targetActor);

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
                this.actor.TimeController.BeginHitStop(this.hitStopTimeScale, this.hitStopDurationSeconds).Forget();
            }

            var partName = targetActor.PartController.GetPart(other.gameObject).PartType;
            var damageRate = targetActor.PartController.GetDamageRate(other.gameObject);
            var damage = Calculator.GetDamage(this.weaponPower, this.motionPower, damageRate);
            targetActor.StatusController.ReceiveDamage(damage, partName, this.actor.transform.position);

            MessageBroker.GetPublisher<Actor, ActorEvents.GaveDamage>()
                .Publish( this.actor, ActorEvents.GaveDamage.Get(damage));
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
                    this.motionPower = x.Data.Power;
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
