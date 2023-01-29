using BehaviorDesigner.Runtime.Tasks;
using MH.ActorControllers;
using Unity.Netcode;

namespace MH.BehaviourDesignerControllers
{
    /// <summary>
    ///
    /// </summary>
    [TaskCategory("MH")]
    public sealed class RequestNetworkNewPosture : Action
    {
        public SharedEnemyBehaviourTreeCore core;

        public override TaskStatus OnUpdate()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return TaskStatus.Success;
            }

            var c = this.core.Value;
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestNetworkNewPosture>()
                .Publish(c.owner, ActorEvents.RequestNetworkNewPosture.Get());

            return TaskStatus.Success;
        }
    }
}
