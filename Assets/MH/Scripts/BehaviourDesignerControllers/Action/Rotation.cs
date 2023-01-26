using BehaviorDesigner.Runtime.Tasks;
using MH.BehaviourDesignerControllers;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("Actor経由で回転処理を行います")]
    public sealed class Rotation : Action
    {
        public SharedActor actor;

        public float rotateSpeed;

        public override TaskStatus OnUpdate()
        {
            var a = this.actor.Value;

            return TaskStatus.Success;
        }
    }
}
