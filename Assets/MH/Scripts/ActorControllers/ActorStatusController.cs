using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    ///
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
        private readonly Dictionary<string, ActorStatus.PartData> basePartDataList = new();

        private readonly Dictionary<string, int> currentEndurances = new();

        public IAsyncReactiveProperty<int> HitPointMax => this.hitPointMax;

        public IAsyncReactiveProperty<int> HitPoint => this.hitPoint;

        public IReadOnlyDictionary<string, ActorStatus.PartData> BasePartDataList => this.basePartDataList;

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
                this.basePartDataList.Add(partData.PartName, partData);
                this.currentEndurances.Add(partData.PartName, 0);
            }
        }

        public void ReceiveDamage(int damage, string partName, Vector3 opposePosition)
        {
            if (this.IsDead)
            {
                return;
            }

            this.hitPoint.Value -= damage;
            MessageBroker.GetPublisher<Actor, ActorEvents.ReceivedDamage>()
                .Publish(this.actor, ActorEvents.ReceivedDamage.Get(damage));

            if (this.IsDead)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.Died>()
                    .Publish(this.actor, ActorEvents.Died.Get());
            }
            else
            {
                this.currentEndurances[partName] += damage;
                if (this.currentEndurances[partName] >= this.basePartDataList[partName].Endurance)
                {
                    this.currentEndurances[partName] = 0;
                    this.actor.StateController.ForceFlinch(opposePosition);
                }
            }
        }

        public float GetPartDamageRate(string partName)
        {
            var result = this.basePartDataList[partName];
            Assert.IsNotNull(result, $"{partName}という部位は存在しません");

            return result.DamageRate;
        }
    }
}
