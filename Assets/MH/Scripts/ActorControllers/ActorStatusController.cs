using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using MH.NetworkSystems;
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

        private readonly AsyncReactiveProperty<float> hitPointMax = new(0);

        private readonly AsyncReactiveProperty<float> hitPoint = new(0);

        private readonly AsyncReactiveProperty<float> staminaMax = new(0);

        private readonly AsyncReactiveProperty<float> stamina = new(0);

        private readonly AsyncReactiveProperty<int> specialGauge = new(0);

        private readonly AsyncReactiveProperty<int> specialTank = new(0);

        private float recoveryHitPoint;

        private float recoveryHitPointConsumePower;

        private float recoveryHitPointMin;

        /// <summary>
        /// 基礎となる部位データ
        /// </summary>
        private readonly Dictionary<Define.PartType, ActorStatus.PartData> basePartDataList = new();

        private readonly Dictionary<Define.PartType, int> currentEndurances = new();

        private readonly Dictionary<Define.PartType, int> flinchCounts = new();

        /// <summary>
        /// 無敵時のキャンセルトークンソース
        /// </summary>
        private CancellationTokenSource invincibleTokenSource;

        public bool IsInvincible { private set; get; }

        /// <summary>
        /// 回復中のキャンセルトークンソース
        /// </summary>
        private CancellationTokenSource recoveryTokenSource;

        /// <summary>
        /// 回復中であるか
        /// </summary>
        public bool IsRecovering => this.recoveryTokenSource != null;

        public IAsyncReactiveProperty<float> HitPointMax => this.hitPointMax;

        public IAsyncReactiveProperty<float> HitPoint => this.hitPoint;

        public IAsyncReactiveProperty<float> StaminaMax => this.staminaMax;

        public IAsyncReactiveProperty<float> Stamina => this.stamina;

        public IAsyncReactiveProperty<int> SpecialGauge => this.specialGauge;

        public IAsyncReactiveProperty<int> SpecialTank => this.specialTank;

        public IReadOnlyDictionary<Define.PartType, ActorStatus.PartData> BasePartDataList => this.basePartDataList;

        public IReadOnlyDictionary<Define.PartType, int> CurrentEndurances => this.currentEndurances;

        public bool IsDead => this.hitPoint.Value <= 0;

        public bool IsHitPointMax => this.hitPoint.Value >= this.hitPointMax.Value;

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
            this.staminaMax.Value = this.BaseStatus.stamina;
            this.stamina.Value = this.BaseStatus.stamina;
            this.recoveryHitPoint = spawnData.recoveryHitPoint;
            this.recoveryHitPointConsumePower = spawnData.recoveryHitPointConsumePower;
            this.recoveryHitPointMin = spawnData.recoveryHitPointMin;
            foreach (var partData in this.BaseStatus.partDataList)
            {
                this.basePartDataList.Add(partData.PartType, partData);
                this.currentEndurances.Add(partData.PartType, 0);
                this.flinchCounts.Add(partData.PartType, 0);
            }

            var ct = this.actor.GetCancellationTokenOnDestroy();
            this.actor.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    var s = this.stamina.Value;
                    s += spawnData.recoveryStamina * TimeManager.Game.deltaTime;
                    s = Mathf.Min(s, this.staminaMax.Value);
                    this.stamina.Value = s;
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.GaveDamage>().Subscribe(this.actor, x =>
                {
                    if (x.Data.canRecoverySpecialCharge)
                    {
                        this.AddSpecialGauge(x.Data.damage);
                    }
                })
                .AddTo(ct);
        }

        public void ReceiveDamage(DamageData damageData, Define.PartType partType, Vector3 opposePosition)
        {
            if (this.IsDead)
            {
                return;
            }

            if (this.IsInvincible)
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
                if (damageData.isGuardSuccess)
                {
                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestUniqueMotion>()
                        .Publish(this.actor, ActorEvents.RequestUniqueMotion.Get("GuardSuccess"));
                }
                else
                {
                    this.currentEndurances[partType] += damageData.damage;
                    this.TryFlinch(partType, opposePosition);
                }
            }
        }

        /// <summary>
        /// 無敵を開始する
        /// </summary>
        public async UniTaskVoid BeginInvincibleAsync(float durationSeconds)
        {
            if (this.invincibleTokenSource != null)
            {
                this.invincibleTokenSource.Cancel();
                this.invincibleTokenSource.Dispose();
            }

            this.IsInvincible = true;
            this.invincibleTokenSource = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(durationSeconds), cancellationToken: this.invincibleTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }

            this.IsInvincible = false;
            this.invincibleTokenSource.Dispose();
            this.invincibleTokenSource = null;
        }

        /// <summary>
        /// スタミナが足りるか返す
        /// </summary>
        public bool IsEnoughStamina()
        {
            return this.stamina.Value > 0;
        }

        /// <summary>
        /// スタミナを使用する
        /// </summary>
        public void UseStamina(float stamina)
        {
            this.stamina.Value -= stamina;
        }

        private void AddSpecialGauge(int value)
        {
            var g = this.specialGauge.Value;
            var t = this.specialTank.Value;
            g += value;
            while (g >= Define.SpecialGaugeMax && t < Define.SpecialTankMax)
            {
                t++;
                g -= Define.SpecialGaugeMax;
            }
            t = Mathf.Min(t, Define.SpecialTankMax);
            g = Mathf.Min(g, Define.SpecialGaugeMax);
            this.specialGauge.Value = g;
            this.specialTank.Value = t;
        }

        public bool CanSpecialAttack()
        {
            return this.specialTank.Value > 0;
        }

        public void UseSpecialAttack()
        {
            if (this.specialTank.Value >= Define.SpecialTankMax && this.specialGauge.Value >= Define.SpecialGaugeMax)
            {
                this.specialGauge.Value = 0;
            }
            else
            {
                this.specialTank.Value--;
            }
        }

        public float GetPartDamageRate(Define.PartType partType)
        {
            var result = this.basePartDataList[partType];
            Assert.IsNotNull(result, $"{partType}という部位は存在しません");

            return result.DamageRate;
        }

        public void BeginRecovery()
        {
            this.EndRecovery();
            this.recoveryTokenSource = new CancellationTokenSource();

            this.actor.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    this.AddHitPoint(this.recoveryHitPoint * TimeManager.Game.deltaTime);
                    this.recoveryHitPoint -= this.recoveryHitPointConsumePower * TimeManager.Game.deltaTime;
                    this.recoveryHitPoint = Mathf.Max(this.recoveryHitPoint, this.recoveryHitPointMin);
                })
                .AddTo(this.recoveryTokenSource.Token);
        }

        public void EndRecovery()
        {
            if (this.recoveryTokenSource != null)
            {
                this.recoveryTokenSource.Cancel();
                this.recoveryTokenSource.Dispose();
                MessageBroker.GetPublisher<Actor, ActorEvents.EndRecovery>()
                    .Publish(this.actor, ActorEvents.EndRecovery.Get());
            }

            this.recoveryTokenSource = null;
        }

        public void AddHitPoint(float value)
        {
            var h = this.hitPoint.Value;
            h += value;
            h = Mathf.Min(h, this.hitPointMax.Value);
            this.hitPoint.Value = h;
        }

        public void SyncHitPoint(NetworkVariable<float> networkHitPoint)
        {
            this.hitPoint.Value = networkHitPoint.Value;
        }

        public void SyncPartDataList(NetworkList<PartDataNetworkVariable> networkPartDataList)
        {
            foreach (var partData in networkPartDataList)
            {
                this.currentEndurances[partData.partType] = partData.endurance;
                this.TryFlinch(partData.partType, partData.opposePosition);
            }
        }

        private void TryFlinch(Define.PartType partType, Vector3 opposePosition)
        {
            if (this.IsDead)
            {
                return;
            }

            if (this.currentEndurances[partType] >= this.basePartDataList[partType].Endurance * (this.flinchCounts[partType] + 1))
            {
                this.flinchCounts[partType]++;
                this.actor.StateController.ForceFlinch(opposePosition);
            }
        }
    }
}
