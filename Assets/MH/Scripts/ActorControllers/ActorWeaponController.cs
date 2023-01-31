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

        private readonly HashSet<Actor> collidedActors = new();

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

            // 連続ヒットを避けるため、既に処理済みのActorは無視する
            if (this.collidedActors.Contains(targetActor))
            {
                return;
            }

            this.collidedActors.Add(targetActor);

            var t = this.transform;
            var hitPosition = other.ClosestPoint(t.position);
            var hitRotation = t.rotation;
            MessageBroker.GetPublisher<PoolablePrefabEvents.RequestCreate>()
                .Publish(PoolablePrefabEvents.RequestCreate.Get(this.hitEffectPrefab, hitPosition, hitRotation));

            if (this.hitStopDurationSeconds > 0.0f)
            {
                this.actor.TimeController.BeginHitStop(this.hitStopTimeScale, this.hitStopDurationSeconds).Forget();
            }

            var partName = targetActor.PartController.GetPart(other.gameObject).PartType;
            var damageRate = targetActor.PartController.GetDamageRate(other.gameObject);
            var damageData = Calculator.GetDamageData(this.weaponPower, this.motionPower, damageRate);
            targetActor.StatusController.ReceiveDamage(damageData, partName, this.actor.transform.position);

            MessageBroker.GetPublisher<Actor, ActorEvents.GaveDamage>()
                .Publish(this.actor, ActorEvents.GaveDamage.Get(damageData));
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
                    this.collidedActors.Clear();
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
