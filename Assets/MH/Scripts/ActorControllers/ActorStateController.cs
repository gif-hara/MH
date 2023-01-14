using MessagePipe;
using UnityEngine.Assertions;

namespace MH
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
        }

        private Actor actor;

        /// <summary>
        /// 次の攻撃のリクエストタイプ
        /// </summary>
        private Define.RequestAttackType nextAttackType;

        /// <summary>
        /// 攻撃時の回転処理を行えるか
        /// </summary>
        private bool onAttackCanRotate;

        private StateController<State> stateController;

        void IActorController.Setup(Actor actor, ActorSpawnData spawnData)
        {
            this.actor = actor;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, OnEnterIdle, null);
            this.stateController.Set(State.Run, OnEnterRun, null);
            this.stateController.Set(State.Dodge, OnEnterDodge, null);
            this.stateController.Set(State.Attack, OnEnterAttack, OnExitAttack);

            this.stateController.ChangeRequest(State.Idle);
        }

        private void OnEnterIdle(State previousState, DisposableBagBuilder scope)
        {
            this.actor.AnimationController.PlayIdle();

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Run);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestMove>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.PostureController.Move(x.Velocity);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.PostureController.Rotate(x.Rotation);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.DodgeController.Ready(
                        x.Direction,
                        x.Speed,
                        x.Duration,
                        x.Ease
                        );
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
        }

        private void OnEnterRun(State previousState, DisposableBagBuilder scope)
        {
            this.actor.AnimationController.PlayRun();

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestMove>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.PostureController.Move(x.Velocity);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.PostureController.Rotate(x.Rotation);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.DodgeController.Ready(
                        x.Direction,
                        x.Speed,
                        x.Duration,
                        x.Ease
                        );
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
        }

        private void OnEnterDodge(State previousState, DisposableBagBuilder scope)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                        .Subscribe(this.actor, x =>
                        {
                            this.nextAttackType = x.AttackType;
                            this.stateController.ChangeRequest(State.Attack);
                        })
                        .AddTo(scope);
                    MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                        .Subscribe(this.actor, x =>
                        {
                            this.actor.DodgeController.Ready(
                                x.Direction,
                                x.Speed,
                                x.Duration,
                                x.Ease
                                );
                            this.stateController.ChangeRequest(State.Dodge);
                        })
                        .AddTo(scope);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndDodge>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            this.actor.DodgeController.Invoke();
        }

        private void OnEnterAttack(State previousState, DisposableBagBuilder scope)
        {
            this.onAttackCanRotate = false;

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.onAttackCanRotate)
                    {
                        return;
                    }

                    this.actor.PostureController.Rotate(x.Rotation);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.onAttackCanRotate = true;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.CloseRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.onAttackCanRotate = false;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptNextState>()
                .Subscribe(this.actor, _ =>
                {
                    MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                        .Subscribe(this.actor, x =>
                        {
                            this.nextAttackType = x.AttackType;
                            this.stateController.ChangeRequest(State.Attack);
                        })
                        .AddTo(scope);
                    MessageBroker.GetSubscriber<Actor, ActorEvents.RequestDodge>()
                        .Subscribe(this.actor, x =>
                        {
                            this.actor.DodgeController.Ready(
                                x.Direction,
                                x.Speed,
                                x.Duration,
                                x.Ease
                                );
                            this.stateController.ChangeRequest(State.Dodge);
                        })
                        .AddTo(scope);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndAttack>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            ActorAttackController.AttackType attackType = ActorAttackController.AttackType.WeakAttack;
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
        }

        private void OnExitAttack(State nextState)
        {
            if (nextState == State.Attack)
            {
                return;
            }

            this.actor.AttackController.Reset();
        }
    }
}
