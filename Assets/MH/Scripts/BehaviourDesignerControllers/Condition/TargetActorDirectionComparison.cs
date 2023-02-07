using BehaviorDesigner.Runtime.Tasks;

namespace MH.BehaviourDesignerControllers
{
    [TaskCategory("MH")]
    [TaskDescription("ターゲットのActorがどこにいるかで比較します")]
    public sealed class TargetActorDirectionComparison : Conditional
    {
        public SharedEnemyActorBehaviour enemy;

        public EnemyActorBehaviour.TargetDirectionType type;

        public override TaskStatus OnUpdate()
        {
            return this.enemy.Value.TargetDirection == this.type
                ? TaskStatus.Success
                : TaskStatus.Failure;
        }

        public override void OnReset()
        {
        }
    }
}
