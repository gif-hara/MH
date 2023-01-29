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
            var c = this.core.Value;
            var index = c.GetRandom(() => Random.Range(0, ActorManager.Players.Count));
            c.targetActor = ActorManager.Players[index];

            return TaskStatus.Success;
        }
    }
}
