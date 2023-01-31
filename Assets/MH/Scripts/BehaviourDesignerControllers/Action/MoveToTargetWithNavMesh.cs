using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;
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
        public SharedEnemyActorBehaviour enemy;

        public float moveSpeed;

        public float rotateSpeed;

        public float rotateOffset;

        public bool canMove;

        public bool canRotate;

        public RotateMode rotateMode;

        /// <summary>
        /// 回転処理のモード
        /// </summary>
        public enum RotateMode
        {
            /// <summary>
            /// NavMeshAgentの目標地点へ向く
            /// </summary>
            NavMeshAgent,

            /// <summary>
            /// 攻撃対象の<see cref="Actor"/>の方へ向く
            /// </summary>
            TargetActor
        }

        public override TaskStatus OnUpdate()
        {
            var e = this.enemy.Value;
            if (e.targetActor == null)
            {
                Debug.LogWarning("攻撃対象が存在しません");
                return TaskStatus.Failure;
            }

            e.navMeshAgent.destination = e.targetActor.transform.position;

            if (this.canMove)
            {
                e.owner.PostureController.Move(
                    this.GetDirectionFromNavMesh() * this.moveSpeed * e.owner.TimeController.Time.deltaTime
                    );
                e.navMeshAgent.nextPosition = e.owner.transform.position;
            }
            if (this.canRotate)
            {
                var direction = this.rotateMode == RotateMode.NavMeshAgent
                    ? this.GetDirectionFromNavMesh()
                    : this.GetDirectionFromTargetActor();
                if (direction != Vector3.zero)
                {
                    var to = Quaternion.LookRotation(direction);
                    to = Quaternion.Euler(to.eulerAngles + new Vector3(0.0f, this.rotateOffset, 0.0f));
                    var rotation = Quaternion.Lerp(
                        e.owner.transform.rotation,
                        to,
                        this.rotateSpeed * e.owner.TimeController.Time.deltaTime
                        );
                    e.owner.PostureController.Rotate(rotation);
                }
            }

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

        private Vector3 GetDirectionFromNavMesh()
        {
            var c = this.enemy.Value;
            var nextPosition = GetNextPosition(c.navMeshAgent.path);
            return Vector3.Scale(
                nextPosition - c.owner.transform.position,
                new Vector3(1.0f, 0.0f, 1.0f)
                ).normalized;
        }

        private Vector3 GetDirectionFromTargetActor()
        {
            var c = this.enemy.Value;
            return Vector3.Scale(
                c.targetActor.transform.position - c.owner.transform.position,
                new Vector3(1.0f, 0.0f, 1.0f)
                ).normalized;
        }
    }
}
