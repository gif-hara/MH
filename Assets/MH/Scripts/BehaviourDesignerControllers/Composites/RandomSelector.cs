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

        [SerializeField]
        private List<WeightData> weights;

        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        private readonly Queue<int> invokeOrder = new();

        private readonly List<int> childIndexList = new();

        private readonly List<WeightData> cachedWeights = new();

        public override void OnAwake()
        {
            this.childIndexList.Clear();
            for (var i = 0; i < this.children.Count; i++)
            {
                this.childIndexList.Add(i);
            }
            this.invokeOrder.Clear();
            this.cachedWeights.Clear();
            this.cachedWeights.AddRange(this.weights);
            for (var i = 0; i < this.children.Count; i++)
            {
                var index = this.cachedWeights.LotteryIndex(max => this.enemy.Value.GetRandomSelector(() => Random.Range(0, max)));
                this.invokeOrder.Enqueue(this.childIndexList[index]);
                this.childIndexList.RemoveAt(index);
                this.cachedWeights.RemoveAt(index);
            }
        }

        public override int CurrentChildIndex()
        {
            var result = this.invokeOrder.Dequeue();
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
