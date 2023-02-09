using System;
using System.Text;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    /// 敵の<see cref="Actor"/>を制御するクラス
    /// </summary>
    public sealed class EnemyActorBehaviour : MonoBehaviour
    {
        public Actor owner;

        public NavMeshAgent navMeshAgent;

        /// <summary>
        /// 攻撃対象となる<see cref="Actor"/>
        /// </summary>
        public Actor targetActor;

        public BehaviorTree entryPointTree;

        private Random.State thinkState;

        private BehaviorTree[] trees;

        /// <summary>
        /// ターゲットがいる向きのタイプ
        /// </summary>
        public enum TargetDirectionType
        {
            Front,
            Back,
            Left,
            Right
        }

        /// <summary>
        /// ターゲットとの距離を返す
        /// </summary>
        /// <remarks>
        /// Behaviour Treeで利用しています
        /// </remarks>
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

        /// <summary>
        /// ターゲットがどこにいるか返す
        /// </summary>
        public TargetDirectionType TargetDirection
        {
            get
            {
                var t = this.owner.transform;
                var lhs = (this.targetActor.transform.position - t.position).normalized;
                var f = Vector3.Dot(lhs, t.forward);
                if (f > 0.5f && f <= 1.0f)
                {
                    return TargetDirectionType.Front;
                }
                if (f < -0.5f && f >= -1.0f)
                {
                    return TargetDirectionType.Back;
                }

                var r = Vector3.Dot(lhs, t.right);
                if (r > 0.5f && r <= 1.0f)
                {
                    return TargetDirectionType.Right;
                }
                if (r < -0.5f && r >= -1.0f)
                {
                    return TargetDirectionType.Left;
                }

                Assert.IsTrue(false, $"未定義の動作です f = {f}, r = {r}");
                return TargetDirectionType.Front;
            }
        }

        /// <summary>
        /// ホストであるか返す
        /// </summary>
        /// <remarks>
        /// Behaviour Treeで利用しています
        /// </remarks>
        public bool IsHost => NetworkManager.Singleton.IsHost;

        private void Awake()
        {
            this.navMeshAgent.updatePosition = false;
            this.navMeshAgent.updateRotation = false;

            this.trees = this.GetComponentsInChildren<BehaviorTree>();
            this.DisableAllBehaviourTrees();

            if (this.IsHost)
            {
                this.entryPointTree.EnableBehavior();
            }

            var ct = this.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<Actor, ActorEvents.ReceivedNewThinkData>()
                .Subscribe(this.owner, x =>
                {
                    this.owner.PostureController.Warp(x.Position);
                    this.owner.PostureController.Rotate(Quaternion.Euler(0.0f, x.RotationY, 0.0f));
                    this.InitState(x.Seed);
                    this.DisableAllBehaviourTrees();
                    this.entryPointTree.EnableBehavior();
                    this.owner.StateController.Change(ActorStateController.State.Idle);
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginFlinch>()
                .Subscribe(this.owner, _ =>
                {
                    this.DisableAllBehaviourTrees();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.Died>()
                .Subscribe(this.owner, _ =>
                {
                    this.DisableAllBehaviourTrees();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndFlinch>()
                .Subscribe(this.owner, _ =>
                {
                    if (this.IsHost)
                    {
                        this.entryPointTree.EnableBehavior();
                    }
                })
                .AddTo(ct);

#if MH_DEBUG
            this.owner.GetAsyncLateUpdateTrigger()
                .Subscribe(_ =>
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"{this.owner.name}");
                    builder.AppendLine($"    HP    = {this.owner.StatusController.HitPoint.Value} / {this.owner.StatusController.HitPointMax.Value}");
                    builder.AppendLine($"    State = {this.owner.StateController.CurrentState}");
                    MessageBroker.GetPublisher<DebugPanelEvents.AppendLine>()
                        .Publish(DebugPanelEvents.AppendLine.Get(builder.ToString()));
                })
                .AddTo(ct);
#endif
        }

        private void OnDrawGizmos()
        {
            foreach (var corner in this.navMeshAgent.path.corners)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(corner, 1.0f);
            }
        }

        public void InitState(int seed)
        {
            var prevState = Random.state;
            Random.InitState(seed);
            this.thinkState = Random.state;
            Random.state = prevState;
        }

        public T GetRandomSelector<T>(Func<T> randomSelector)
        {
            var prevState = Random.state;
            Random.state = this.thinkState;
            var result = randomSelector();
            this.thinkState = Random.state;
            Random.state = prevState;

            return result;
        }

        private void DisableAllBehaviourTrees()
        {
            foreach (var behaviorTree in this.trees)
            {
                behaviorTree.DisableBehavior();
            }
        }
    }
}
