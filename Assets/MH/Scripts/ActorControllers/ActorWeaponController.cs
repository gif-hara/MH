using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.ProjectileSystems;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorController, IProjectileController
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private int weaponPower;

        [SerializeField, Range(0, 100)]
        private int criticalRate;

        [SerializeField]
        private List<GameObject> colliders;

        private PoolablePrefab hitEffectPrefab;

        private float hitStopTimeScale;

        private float hitStopDurationSeconds;

        private int motionPower;

        private bool canRecoverySpecialCharge;

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

            if (targetActor == null)
            {
                // ステージなどに当たった想定でエフェクトのみ生成する
                this.CreateEffect(other);
                return;
            }

            // 連続ヒットを避けるため、既に処理済みのActorは無視する
            if (this.collidedActors.Contains(targetActor))
            {
                return;
            }

            this.collidedActors.Add(targetActor);

            this.CreateEffect(other);

            if (this.hitStopDurationSeconds > 0.0f)
            {
                this.actor.TimeController.BeginHitStop(this.hitStopTimeScale, this.hitStopDurationSeconds).Forget();
            }

            if (this.actor.NetworkController.IsOwner || this.actor.StatusController.BaseStatus.actorType == Define.ActorType.Enemy)
            {
                var partType = targetActor.PartController.GetPart(other.gameObject).PartType;
                var damageRate = targetActor.PartController.GetDamageRate(other.gameObject);
                var damageData = Calculator.GetDamageData(
                    this.weaponPower,
                    this.motionPower,
                    damageRate,
                    this.criticalRate,
                    targetActor,
                    this.actor.transform.position,
                    partType,
                    this.canRecoverySpecialCharge
                    );
                targetActor.StatusController.ReceiveDamage(damageData, partType, this.actor.transform.position);

                MessageBroker.GetPublisher<Actor, ActorEvents.GaveDamage>()
                    .Publish(this.actor, ActorEvents.GaveDamage.Get(damageData));
            }
        }

        private void CreateEffect(Collider other)
        {
            var t = this.transform;
            var hitPosition = other.ClosestPoint(t.position);
            var hitRotation = t.rotation;
            MessageBroker.GetPublisher<PoolablePrefabEvents.RequestCreate>()
                .Publish(PoolablePrefabEvents.RequestCreate.Get(this.hitEffectPrefab, hitPosition, hitRotation));
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
                    this.canRecoverySpecialCharge = x.Data.CanRecoverySpecialCharge;
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.InvalidationAttackCollider>()
                .Subscribe(this.actor, x =>
                {
                    this.colliderDictionary[x.ColliderName].SetActive(false);
                })
                .AddTo(ct);
        }

        public void Setup(Projectile projectile, ProjectileData data, Actor actor)
        {
            this.actor = actor;
            this.hitStopDurationSeconds = 0.0f;
            foreach (var o in this.colliders)
            {
                o.SetActive(true);
            }
            this.collidedActors.Clear();
            this.motionPower = data.motionPower;
            this.hitEffectPrefab = data.hitEffectPrefab;
        }
    }
}
