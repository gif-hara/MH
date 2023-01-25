using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("攻撃対象となるActorを設定します")]
    public sealed class SetTargetActor : Action
    {
        public SharedEnemyBehaviourTreeCore core;

        public override TaskStatus OnUpdate()
        {
            if (this.core.Value.targetActor != null)
            {
                return TaskStatus.Success;
            }

            this.core.Value.targetActor = ActorManager.Players[Random.Range(0, ActorManager.Players.Count)];

            return TaskStatus.Success;
        }
    }
}
