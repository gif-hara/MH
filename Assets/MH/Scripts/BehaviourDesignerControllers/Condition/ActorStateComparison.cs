using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;

namespace MH.BehaviourDesignerControllers
{
    [TaskCategory("MH")]
    [TaskDescription("ActorのStateの比較を行います")]
    public sealed class ActorStateComparison : Conditional
    {
        public SharedActor actor;

        public ActorStateController.State state;

        public override TaskStatus OnUpdate()
        {
            return this.actor.Value.StateController.CurrentState == this.state ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            this.actor = null;
            this.state = ActorStateController.State.Idle;
        }
    }
}
