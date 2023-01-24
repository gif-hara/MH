using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorStateController : IActorController
    {
        public enum State
        {
            Invalid,
            Idle,
            Run,
            Dodge,
            Attack,
            UniqueMotion,
        }

        private Actor actor;

        private bool canNextState;

        /// <summary>
        /// 次の攻撃のリクエストタイプ
        /// </summary>
        private Define.RequestAttackType nextAttackType;

        private StateController<State> stateController;

        private string uniqueMotionName;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, this.OnEnterIdle, null);
            this.stateController.Set(State.Run, this.OnEnterRun, null);
            this.stateController.Set(State.Dodge, this.OnEnterDodge, null);
            this.stateController.Set(State.Attack, this.OnEnterAttack, this.OnExitAttack);
            this.stateController.Set(State.UniqueMotion, this.OnEnterUniqueMotion, null);

            var ct = this.actor.GetCancellationTokenOnDestroy();

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestUniqueMotion>()
                .Subscribe(this.actor, x =>
                {
                    this.uniqueMotionName = x.MotionName;
                    this.stateController.ChangeRequest(State.UniqueMotion);
                })
                .AddTo(ct);

            // 強制的な回避リクエストの場合は受け付ける
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!x.IsForce)
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.stateController.ChangeRequest(State.Dodge);
                })
                .AddTo(ct);

            this.stateController.ChangeRequest(State.Idle);
        }

        private void OnEnterIdle(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Run);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.DodgeController.Ready(x.Data);
                    this.stateController.ChangeRequest(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    this.nextAttackType = x.AttackType;
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(scope);

            this.actor.AnimationController.Play("Idle");
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
        }

        private void OnEnterRun(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.EndMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.DodgeController.Ready(x.Data);
                    this.stateController.ChangeRequest(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    this.nextAttackType = x.AttackType;
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(scope);

            this.actor.AnimationController.Play("Run");
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
        }

        private void OnEnterDodge(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    this.canNextState = true;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    this.nextAttackType = x.AttackType;
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.stateController.ChangeRequest(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndDodge>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            this.actor.DodgeController.Invoke();
            this.canNextState = false;
        }

        private void OnEnterAttack(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    this.canNextState = true;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    this.nextAttackType = x.AttackType;
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.stateController.ChangeRequest(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndAttack>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            var attackType = ActorAttackController.AttackType.WeakAttack;
            if (this.nextAttackType == Define.RequestAttackType.Weak)
            {
                if (previousState == State.Dodge)
                {
                    attackType = ActorAttackController.AttackType.DodgeAttack;
                }
                else
                {
                    attackType = ActorAttackController.AttackType.WeakAttack;
                }
            }
            else if (this.nextAttackType == Define.RequestAttackType.Strong)
            {
                attackType = ActorAttackController.AttackType.StrongAttack;
            }
            else
            {
                Assert.IsTrue(false, this.nextAttackType.ToString());
            }

            this.actor.AttackController.Invoke(attackType);
            this.canNextState = false;
        }

        private void OnExitAttack(State nextState)
        {
            if (nextState == State.Attack)
            {
                return;
            }

            this.actor.AttackController.Reset();
        }

        private async void OnEnterUniqueMotion(State prevState, DisposableBagBuilder scope)
        {
            try
            {
                await this.actor.AnimationController.PlayAsync(this.uniqueMotionName);
                this.stateController.ChangeRequest(State.Idle);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
