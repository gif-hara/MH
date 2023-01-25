using MH.ActorControllers;
using UnityEngine;
using UnityEngine.AI;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    /// 敵アクターとBehaviour Designerとを繫げこむクラス
    /// </summary>
    public sealed class EnemyBehaviourTreeCore : MonoBehaviour
    {
        public Actor owner;

        public NavMeshAgent navMeshAgent;

        /// <summary>
        /// 攻撃対象となる<see cref="Actor"/>
        /// </summary>
        public Actor targetActor;

        public float TargetDistance
        {
            get
            {
                if (this.targetActor == null)
                {
                    Debug.LogWarning("攻撃対象が存在しません");
                    return -1;
                }

                var result = Vector3.Distance(this.owner.transform.position, this.targetActor.transform.position);
                return result;
            }
        }

        private void Awake()
        {
            this.navMeshAgent.updatePosition = false;
            this.navMeshAgent.updateRotation = false;
        }

        private void OnDrawGizmos()
        {
            foreach (var corner in this.navMeshAgent.path.corners)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(corner, 1.0f);
            }
        }
    }
}
