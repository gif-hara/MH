using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    [TaskCategory("MH")]
    [TaskDescription("ランダムにSuccessを返します")]
    public sealed class RandomSuccess : Conditional
    {
        public SharedEnemyActorBehaviour enemy;

        [Range(0.0f, 1.0f)]
        public float range;

        public override TaskStatus OnUpdate()
        {
            var e = this.enemy.Value;
            var result = e.GetRandomSelector(() => Random.value) <= this.range ? TaskStatus.Success : TaskStatus.Failure;
            return result;
        }

        public override void OnReset()
        {
        }
    }
}
