using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorStateController
    {
        public enum State
        {
            Invalid,
            Idle,
            Run,
            Dodge,
            WeakAttack,
        }

        private readonly Actor actor;

        private readonly StateController<State> stateController;

        /// <summary>
        /// 攻撃時の回転処理を行えるか
        /// </summary>
        private bool onAttackCanRotate = false;
        
        public ActorStateController(Actor actor)
        {
            this.actor = actor;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, OnEnterIdle, null);
            this.stateController.Set(State.Run, OnEnterRun, null);
            this.stateController.Set(State.Dodge, OnEnterDodge, null);
            this.stateController.Set(State.WeakAttack, OnEnterWeakAttack, null);
            
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
                    this.actor.DodgeController.Invoke(
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
                    this.stateController.ChangeRequest(State.WeakAttack);
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
                    this.actor.DodgeController.Invoke(
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
                    this.stateController.ChangeRequest(State.WeakAttack);
                })
                .AddTo(scope);
        }

        private async void OnEnterDodge(State previousState, DisposableBagBuilder scope)
        {
            await this.actor.AnimationController.PlayDodgeAsync();
            
            this.stateController.ChangeRequest(State.Idle);
        }
        
        private async void OnEnterWeakAttack(State previousState, DisposableBagBuilder scope)
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
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.WeakAttack);
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndAttack>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(scope);

            this.actor.AttackController.Invoke(ActorAttackController.AttackType.WeakAttack);
        }
    }
}
