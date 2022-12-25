using MessagePipe;
using UnityEngine;
using UnityEngine.Assertions;

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
        }

        private Actor actor;

        private StateController<State> stateController;
        
        public ActorStateController(Actor actor)
        {
            this.actor = actor;
            this.stateController = new StateController<State>(State.Invalid);
            this.stateController.Set(State.Idle, OnEnterIdle, null);
            this.stateController.Set(State.Run, OnEnterRun, null);
            
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
        }
    }
}
