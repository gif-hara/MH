using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// </summary>
    public sealed class ActorStatusController : IActorController
    {
        private Actor actor;

        public ActorStatus BaseStatus { private set; get; }

        private readonly AsyncReactiveProperty<int> hitPointMax = new(0);

        private readonly AsyncReactiveProperty<int> hitPoint = new(0);

        /// <summary>
        /// 基礎となる部位データ
        /// </summary>
        private readonly Dictionary<Define.PartType, ActorStatus.PartData> basePartDataList = new();

        private readonly Dictionary<Define.PartType, int> currentEndurances = new();

        public IAsyncReactiveProperty<int> HitPointMax => this.hitPointMax;

        public IAsyncReactiveProperty<int> HitPoint => this.hitPoint;

        public IReadOnlyDictionary<Define.PartType, ActorStatus.PartData> BasePartDataList => this.basePartDataList;

        public bool IsDead => this.hitPoint.Value <= 0;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            this.BaseStatus = new ActorStatus(spawnData.actorStatus);
            this.hitPointMax.Value = this.BaseStatus.hitPoint;
            this.hitPoint.Value = this.BaseStatus.hitPoint;
            foreach (var partData in this.BaseStatus.partDataList)
            {
                this.basePartDataList.Add(partData.PartType, partData);
                this.currentEndurances.Add(partData.PartType, 0);
            }
        }

        public void ReceiveDamage(DamageData damageData, Define.PartType partType, Vector3 opposePosition)
        {
            if (this.IsDead)
            {
                return;
            }

            this.hitPoint.Value -= damageData.damage;
            MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedDamage>()
                .Publish(this.actor, ActorEvents.ReceivedDamage.Get(damageData));

            if (this.IsDead)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.Died>()
                    .Publish(this.actor, ActorEvents.Died.Get());
            }
            else
            {
                this.currentEndurances[partType] += damageData.damage;
                if (this.currentEndurances[partType] >= this.basePartDataList[partType].Endurance)
                {
                    this.currentEndurances[partType] = 0;
                    this.actor.StateController.ForceFlinch(opposePosition);
                }
            }
        }

        public float GetPartDamageRate(Define.PartType partType)
        {
            var result = this.basePartDataList[partType];
            Assert.IsNotNull(result, $"{partType}という部位は存在しません");

            return result.DamageRate;
        }

        public void SyncHitPoint(NetworkVariable<int> networkHitPoint)
        {
            this.hitPoint.Value = networkHitPoint.Value;
        }
    }
}
