using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    [TaskDescription("ランダムな子要素を選択します")]
    [TaskIcon("{SkinColor}RandomSelectorIcon.png")]
    [TaskCategory("MH")]
    public class RandomSelector : Composite
    {
        public SharedEnemyActorBehaviour enemy;

        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        private readonly Queue<int> invokeOrder = new();

        private readonly List<int> childIndexList = new();

        public override void OnAwake()
        {
            this.childIndexList.Clear();
            for (var i = 0; i < this.children.Count; i++)
            {
                this.childIndexList.Add(i);
            }
            this.invokeOrder.Clear();
            for (var i = 0; i < this.children.Count; i++)
            {
                var index = this.enemy.Value.GetRandomSelector(() => Random.Range(0, this.childIndexList.Count));
                this.invokeOrder.Enqueue(this.childIndexList[index]);
                this.childIndexList.RemoveAt(index);
            }
        }

        public override int CurrentChildIndex()
        {
            var result = this.invokeOrder.Dequeue();
            Debug.Log(result);
            return result;
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
