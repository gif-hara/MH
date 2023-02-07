using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("攻撃対象となるActorを設定します")]
    public sealed class SetTargetActor : Action
    {
        public SharedEnemyActorBehaviour enemy;

        public override TaskStatus OnUpdate()
        {
            var e = this.enemy.Value;
            var index = e.GetRandomSelector(() => Random.Range(0, ActorManager.Players.Count));
            e.targetActor = ActorManager.Players[index];

            return TaskStatus.Success;
        }
    }
}
