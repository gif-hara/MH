using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    [TaskDescription("ランダムな子要素を選択します")]
    [TaskIcon("{SkinColor}RandomSelectorIcon.png")]
    [TaskCategory("MH")]
    public class RandomSelector : Composite
    {
        public SharedEnemyBehaviourTreeCore core;

        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override int CurrentChildIndex()
        {
            return this.core.Value.GetRandom(() => Random.Range(0, this.children.Count));
        }

        public override bool CanExecute()
        {
            return executionStatus != TaskStatus.Success;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Start from the beginning on an abort
            executionStatus = TaskStatus.Inactive;
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
        }
    }
}
