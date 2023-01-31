using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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

        public BehaviorTree entryPointTree;

        private Random.State thinkState;

        private List<BehaviorTree> trees;

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
        /// ホストであるか返す
        /// </summary>
        /// <remarks>
        /// Behaviour Treeで利用しています
        /// </remarks>
        public bool IsHost => NetworkManager.Singleton.IsHost;

        private void Start()
        {
            this.navMeshAgent.updatePosition = false;
            this.navMeshAgent.updateRotation = false;

            this.trees = this.GetComponentsInChildren<BehaviorTree>().ToList();
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
                    this.owner.StateController.ForceChange(ActorStateController.State.Idle);
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginFlinch>()
                .Subscribe(this.owner, x =>
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

        public T GetRandom<T>(Func<T> randomSelector)
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
