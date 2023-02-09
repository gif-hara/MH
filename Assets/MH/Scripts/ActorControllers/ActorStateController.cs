using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

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
            RecoveryBegin,
            RecoveryEnd,
            Dead,
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
        private bool onFlinchRotationOppose;

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
            this.stateController.Set(State.RecoveryBegin, this.OnEnterRecoveryBegin, this.OnExitRecoveryBegin);
            this.stateController.Set(State.RecoveryEnd, this.OnEnterRecoveryEnd, null);
            this.stateController.Set(State.Dead, this.OnEnterDead, null, 3);

            var ct = this.actor.GetCancellationTokenOnDestroy();

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestUniqueMotion>()
                .Subscribe(this.actor, x =>
                {
                    this.uniqueMotionName = x.MotionName;
                    this.Change(State.UniqueMotion);
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
                    this.Change(State.Dodge);
                })
                .AddTo(ct);

            // ガードは常に受け付ける
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestBeginGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.GuardController.Begin();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestEndGuard>()
                .Subscribe(this.actor, _ =>
                {
                    this.actor.GuardController.End();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.Died>()
                .Subscribe(this.actor, _ =>
                {
                    this.Change(State.Dead);
                })
                .AddTo(ct);

            this.Change(State.Idle);
            this.stateController.onChanged += (previousState, currentState) =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.ChangedState>()
                    .Publish(this.actor, ActorEvents.ChangedState.Get(previousState, currentState));
            };
        }

        public void Change(State state)
        {
            this.stateController.ChangeRequest(state).Forget();
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

            this.Change(State.Flinch);
        }

        private void ToIdle()
        {
            if (this.actor.PostureController.IsMoving)
            {
                this.Change(State.Run);
            }
            else
            {
                this.Change(State.Idle);
            }
        }

        private void OnEnterIdle(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.Change(State.Run);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.actor.StatusController.IsEnoughStamina())
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.Change(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (x.AttackType == Define.RequestAttackType.Strong && !this.actor.StatusController.CanSpecialAttack())
                    {
                        return;
                    }
                    this.nextAttackType = x.AttackType;
                    this.Change(State.Attack);
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

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestBeginRecovery>()
                .Subscribe(this.actor, _ =>
                {
                    if (this.actor.StatusController.IsHitPointMax)
                    {
                        return;
                    }
                    this.Change(State.RecoveryBegin);
                })
                .AddTo(scope);

            this.actor.AnimationController.Play(GetAnimationName());
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
            this.actor.GuardController.Validate();
            if (this.actor.PostureController.IsMoving)
            {
                this.Change(State.Run);
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
                    this.Change(State.Idle);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.actor.StatusController.IsEnoughStamina())
                    {
                        return;
                    }

                    this.actor.DodgeController.Ready(x.Data);
                    this.Change(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (x.AttackType == Define.RequestAttackType.Strong && !this.actor.StatusController.CanSpecialAttack())
                    {
                        return;
                    }
                    this.nextAttackType = x.AttackType;
                    this.Change(State.Attack);
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

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestBeginRecovery>()
                .Subscribe(this.actor, _ =>
                {
                    if (this.actor.StatusController.IsHitPointMax)
                    {
                        return;
                    }
                    this.Change(State.RecoveryBegin);
                })
                .AddTo(scope);

            this.actor.AnimationController.Play(GetAnimationName());
            this.actor.PostureController.CanMove = true;
            this.actor.PostureController.CanRotation = true;
            if (!this.actor.PostureController.IsMoving)
            {
                this.Change(State.Idle);
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
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    if (x.AttackType == Define.RequestAttackType.Strong && !this.actor.StatusController.CanSpecialAttack())
                    {
                        return;
                    }

                    this.nextAttackType = x.AttackType;
                    this.Change(State.Attack);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState || !this.actor.StatusController.IsEnoughStamina())
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.Change(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndDodge>()
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
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState)
                    {
                        return;
                    }
                    if (x.AttackType == Define.RequestAttackType.Strong && !this.actor.StatusController.CanSpecialAttack())
                    {
                        return;
                    }

                    this.nextAttackType = x.AttackType;
                    this.Change(State.Attack);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canNextState || !this.actor.StatusController.IsEnoughStamina())
                    {
                        return;
                    }
                    this.actor.DodgeController.Ready(x.Data);
                    this.Change(State.Dodge);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndAttack>()
                .Subscribe(this.actor, _ =>
                {
                    this.ToIdle();
                })
                .AddTo(scope);

            this.actor.AttackController.Invoke(this.nextAttackType, previousState);
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

        private async void OnEnterRecoveryBegin(State previousState, DisposableBagBuilder scope)
        {
            try
            {
                var isRequestEnd = false;
                MessageBroker.GetSubscriber<Actor, ActorEvents.RequestEndRecovery>()
                    .Subscribe(this.actor, _ =>
                    {
                        if (this.actor.StatusController.IsRecovering)
                        {
                            isRequestEnd = true;
                        }
                    })
                    .AddTo(scope);
                this.actor.AnimationController.Play("Recovery.Begin");
                this.actor.PostureController.CanMove = false;
                this.actor.PostureController.CanRotation = false;
                MessageBroker.GetPublisher<Actor, ActorEvents.BeginRecovery>()
                    .Publish(this.actor, ActorEvents.BeginRecovery.Get());

                await UniTask.WaitWhile(() =>
                {
                    if (this.actor.StatusController.IsHitPointMax)
                    {
                        return false;
                    }
                    return !this.actor.StatusController.IsRecovering
                           || !isRequestEnd;
                });
                this.Change(State.RecoveryEnd);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private void OnExitRecoveryBegin(State nextState)
        {
            this.actor.StatusController.EndRecovery();
        }

        private async void OnEnterRecoveryEnd(State previousState, DisposableBagBuilder scope)
        {
            try
            {
                this.actor.StatusController.EndRecovery();
                await this.actor.AnimationController.PlayAsync("Recovery.End");
                this.ToIdle();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private void OnEnterDead(State previousState, DisposableBagBuilder scope)
        {
            this.stateController.IsAccept = false;
            this.actor.AnimationController.Play("Dead");
        }
    }
}
