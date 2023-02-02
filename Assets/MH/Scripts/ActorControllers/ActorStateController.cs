using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
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
            Flinch,
        }

        private Actor actor;

        private bool canNextState;

        /// <summary>
        /// 次の攻撃のリクエストタイプ
        /// </summary>
        private Define.RequestAttackType nextAttackType;

        private StateController<State> stateController;

        private string uniqueMotionName;

        /// <summary>
        /// ひるんだ時に相手の方向へ向き直すか
        /// </summary>
        public bool onFlinchRotationOppose;

        public State CurrentState => this.stateController.CurrentState;

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
            this.onFlinchRotationOppose = spawnData.onFlinchRotationOppose;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, this.OnEnterIdle, null);
            this.stateController.Set(State.Run, this.OnEnterRun, null);
            this.stateController.Set(State.Dodge, this.OnEnterDodge, null);
            this.stateController.Set(State.Attack, this.OnEnterAttack, this.OnExitAttack);
            this.stateController.Set(State.UniqueMotion, this.OnEnterUniqueMotion, null, 1);
            this.stateController.Set(State.Flinch, this.OnEnterFlinch, null, 2);

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

            // ガードは常に受け付ける
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestBeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.GuardController.Begin().Forget();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestEndGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.GuardController.End();
                })
                .AddTo(ct);

            this.stateController.ChangeRequest(State.Idle);
            this.stateController.onChanged += (previousState, currentState) =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.ChangedState>()
                    .Publish(this.actor, ActorEvents.ChangedState.Get(previousState, currentState));
            };
        }

        public void ForceChange(State state)
        {
            this.stateController.ChangeRequest(state);
        }

        public void ForceFlinch(Vector3 opposePosition)
        {
            if (this.onFlinchRotationOppose)
            {
                var direction = Vector3.Scale(
                    opposePosition - this.actor.transform.position,
                    new Vector3(1.0f, 0.0f, 1.0f)
                    );
                this.actor.PostureController.Rotate(Quaternion.LookRotation(direction), true);
            }

            this.stateController.ChangeRequest(State.Flinch);
        }

        private void ToIdle()
        {
            if (this.actor.PostureController.IsMoving)
            {
                this.ForceChange(State.Run);
            }
            else
            {
                this.ForceChange(State.Idle);
            }
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

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.AnimationController.Play(GetAnimationName());
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.AnimationController.Play(GetAnimationName());
                })
                .AddTo(scope);

            this.actor.AnimationController.Play(GetAnimationName());
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
            if (this.actor.PostureController.IsMoving)
            {
                this.stateController.ChangeRequest(State.Run);
            }

            string GetAnimationName()
            {
                return this.actor.GuardController.Guarding ? "GuardIdle" : "Idle";
            }
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

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.AnimationController.Play(GetAnimationName());
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.AnimationController.Play(GetAnimationName());
                })
                .AddTo(scope);

            this.actor.AnimationController.Play(GetAnimationName());
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
            if (!this.actor.PostureController.IsMoving)
            {
                this.stateController.ChangeRequest(State.Idle);
            }

            string GetAnimationName()
            {
                return this.actor.GuardController.Guarding ? "GuardRun" : "Run";
            }
        }

        private void OnEnterDodge(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    this.canNextState = true;
                    this.actor.GuardController.Validate();
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
                    this.ToIdle();
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.ToIdle();
                })
                .AddTo(scope);

            this.actor.DodgeController.Invoke();
            this.canNextState = false;
            this.actor.GuardController.Invalidate();
        }

        private void OnEnterAttack(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    this.canNextState = true;
                    this.actor.GuardController.Validate();
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
                    this.ToIdle();
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.ToIdle();
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
            this.actor.GuardController.Invalidate();
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
                this.ToIdle();
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

        private async void OnEnterFlinch(State previousState, DisposableBagBuilder scope)
        {
            try
            {
                this.actor.PostureController.CanMove = false;
                this.actor.PostureController.CanRotation = false;
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginFlinch>()
                    .Publish(this.actor, ActorEvents.BeginFlinch.Get());
                await this.actor.AnimationController.PlayAsync("Flinch.0");
                this.actor.PostureController.CanMove = true;
                this.actor.PostureController.CanRotation = true;
                this.ToIdle();
                MessageBroker.GetPublisher<Actor, ActorEvents.EndFlinch>()
                    .Publish(this.actor, ActorEvents.EndFlinch.Get());
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
