using BehaviorDesigner.Runtime;

namespace MH.BehaviourDesignerControllers
{
    public sealed class SharedEnemyBehaviourTreeCore : SharedVariable<EnemyBehaviourTreeCore>
    {
        public static implicit operator SharedEnemyBehaviourTreeCore(EnemyBehaviourTreeCore value) { return new SharedEnemyBehaviourTreeCore { mValue = value }; }
    }
}
