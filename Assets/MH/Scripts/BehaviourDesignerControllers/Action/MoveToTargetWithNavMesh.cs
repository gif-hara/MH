using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    [TaskDescription("NavMeshを利用して攻撃対象へと移動します")]
    public sealed class MoveToTargetWithNavMesh : Action
    {
        public SharedEnemyBehaviourTreeCore core;

        public float moveSpeed;

        public float rotateSpeed;

        public override TaskStatus OnUpdate()
        {
            var c = this.core.Value;
            if (c.targetActor == null)
            {
                Debug.LogWarning("攻撃対象が存在しません");
                return TaskStatus.Failure;
            }

            c.navMeshAgent.destination = c.targetActor.transform.position;
            var nextPosition = GetNextPosition(c.navMeshAgent.path);
            var direction = Vector3.Scale(
                nextPosition - c.owner.transform.position,
                new Vector3(1.0f, 0.0f, 1.0f)
                ).normalized;
            c.owner.PostureController.Move(
                direction * this.moveSpeed * c.owner.TimeController.Time.deltaTime
                );
            var rotation = Quaternion.Lerp(
                c.owner.transform.rotation,
                Quaternion.LookRotation(direction),
                this.rotateSpeed * c.owner.TimeController.Time.deltaTime
                );
            c.owner.PostureController.Rotate(rotation);
            c.navMeshAgent.nextPosition = c.owner.transform.position;

            return TaskStatus.Success;
        }

        private static Vector3 GetNextPosition(NavMeshPath path)
        {
            if (path.corners.Length >= 2)
            {
                return path.corners[1];
            }

            return path.corners[0];
        }
    }
}
