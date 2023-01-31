using BehaviorDesigner.Runtime;

namespace MH.BehaviourDesignerControllers
{
    public sealed class SharedEnemyActorBehaviour : SharedVariable<EnemyActorBehaviour>
    {
        public static implicit operator SharedEnemyActorBehaviour(EnemyActorBehaviour value) { return new SharedEnemyActorBehaviour { mValue = value }; }
    }
}
