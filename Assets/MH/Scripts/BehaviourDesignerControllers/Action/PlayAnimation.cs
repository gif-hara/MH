using System;
using BehaviorDesigner.Runtime.Tasks;
using MessagePipe;
using MH.ActorControllers;
using MH.BehaviourDesignerControllers;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace MH
{
    /// <summary>
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("Actor経由でアニメーションを再生します")]
    public sealed class PlayAnimation : Action
    {
        public SharedActor actor;

        public string animationName;

        private IDisposable scope;

        private TaskStatus taskStatus;

        public override void OnAwake()
        {
            base.OnAwake();
            this.taskStatus = TaskStatus.Inactive;
        }

        public override TaskStatus OnUpdate()
        {
            var a = this.actor.Value;

            if (this.taskStatus == TaskStatus.Inactive)
            {
                this.taskStatus = TaskStatus.Failure;
                this.scope = MessageBroker.GetSubscriber<Actor, ActorEvents.ChangedState>()
                    .Subscribe(a, x =>
                    {
                        if (a.StateController.CurrentState != ActorStateController.State.UniqueMotion)
                        {
                            this.scope?.Dispose();
                            this.taskStatus = TaskStatus.Success;
                        }
                    });
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestUniqueMotion>()
                    .Publish(a, ActorEvents.RequestUniqueMotion.Get(this.animationName));
            }

            if (this.taskStatus == TaskStatus.Success)
            {
                this.taskStatus = TaskStatus.Inactive;
                return TaskStatus.Success;
            }

            return this.taskStatus;
        }

        public override void OnReset()
        {
            base.OnReset();
            this.taskStatus = TaskStatus.Inactive;
        }
    }
}
