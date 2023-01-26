using System;
using BehaviorDesigner.Runtime.Tasks;
using MessagePipe;
using MH.ActorControllers;
using MH.BehaviourDesignerControllers;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("Actor経由でアニメーションを再生します")]
    public sealed class PlayAnimation : Action
    {
        public SharedActor actor;

        public string animationName;

        private bool isInitialized;

        private IDisposable scope;

        public override TaskStatus OnUpdate()
        {
            var a = this.actor.Value;

            if (!this.isInitialized)
            {
                this.isInitialized = true;
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestUniqueMotion>()
                    .Publish(a, ActorEvents.RequestUniqueMotion.Get(this.animationName));
                this.scope = MessageBroker.GetSubscriber<Actor, ActorEvents.ChangedState>()
                    .Subscribe(a, x =>
                    {
                        if (x.PreviousState == ActorStateController.State.UniqueMotion)
                        {
                            this.scope?.Dispose();
                            this.isInitialized = false;
                        }
                    });
            }

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            this.isInitialized = false;
        }
    }
}
