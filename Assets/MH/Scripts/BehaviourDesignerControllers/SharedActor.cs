using BehaviorDesigner.Runtime;
using MH.ActorControllers;

namespace MH.BehaviourDesignerControllers
{
    public sealed class SharedActor : SharedVariable<Actor>
    {
        public static implicit operator SharedActor(Actor value) { return new SharedActor { mValue = value }; }
    }
}
