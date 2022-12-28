using System;
using DG.Tweening;
using MessagePipe;
using UniRx;
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
            Attack,
        }

        private Actor actor;

        private StateController<State> stateController;
        
        public ActorStateController(Actor actor)
        {
            this.actor = actor;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, OnEnterIdle, null);
            this.stateController.Set(State.Run, OnEnterRun, null);
            this.stateController.Set(State.Dodge, OnEnterDodge, null);
            this.stateController.Set(State.Attack, OnEnterAttack, null);
            
            this.stateController.ChangeRequest(State.Idle);
        }

        private void OnEnterIdle(State previousState, DisposableBagBuilder bag)
        {
            this.actor.AnimationController.PlayIdle();
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Run);
                })
                .AddTo(bag);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestMove>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.MoveController.Move(x.Velocity);
                })
                .AddTo(bag);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.MoveController.Rotate(x.Rotation);
                })
                .AddTo(bag);

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
                .AddTo(bag);

            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(bag);
        }

        private void OnEnterRun(State previousState, DisposableBagBuilder bag)
        {
            this.actor.AnimationController.PlayRun();
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.EndMove>()
                .Subscribe(this.actor, _ =>
                {
                    this.stateController.ChangeRequest(State.Idle);
                })
                .AddTo(bag);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestMove>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.MoveController.Move(x.Velocity);
                })
                .AddTo(bag);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    this.actor.MoveController.Rotate(x.Rotation);
                })
                .AddTo(bag);

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
                .AddTo(bag);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestAttack>()
                .Subscribe(this.actor, x =>
                {
                    this.stateController.ChangeRequest(State.Attack);
                })
                .AddTo(bag);
        }

        private async void OnEnterDodge(State previousState, DisposableBagBuilder bag)
        {
            await this.actor.AnimationController.PlayDodgeAsync();
            
            this.stateController.ChangeRequest(State.Idle);
        }
        
        private async void OnEnterAttack(State previousState, DisposableBagBuilder bag)
        {
            await this.actor.AttackController.InvokeAsync();
            
            this.stateController.ChangeRequest(State.Idle);
        }
    }
}
