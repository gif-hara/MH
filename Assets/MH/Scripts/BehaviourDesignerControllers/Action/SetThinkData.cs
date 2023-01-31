using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    public sealed class SetThinkData : Action
    {
        public SharedEnemyActorBehaviour enemy;

        public override TaskStatus OnUpdate()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return TaskStatus.Success;
            }

            var e = this.enemy.Value;
            var t = e.owner.transform;
            var seed = (int)(Random.value * 100000000);
            e.InitState(seed);
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestSubmitNewThinkData>()
                .Publish(e.owner, ActorEvents.RequestSubmitNewThinkData.Get(t.position, t.rotation.eulerAngles.y, seed));

            return TaskStatus.Success;
        }
    }
}
