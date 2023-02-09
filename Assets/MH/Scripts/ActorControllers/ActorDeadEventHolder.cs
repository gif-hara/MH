using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の死亡時にイベントを持つクラス
    /// </summary>
    public sealed class ActorDeadEventHolder : MonoBehaviour, IActorController
    {
        [SerializeField]
        private GameObject eventPrefab;

        public void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            MessageBroker.GetSubscriber<Actor, ActorEvents.Died>()
                .Subscribe(actor, _ =>
                {
                    var t = actor.transform;
                    var eventObject = Instantiate(this.eventPrefab, t.position, t.rotation);
                    foreach (var controller in eventObject.GetComponentsInChildren<IActorController>())
                    {
                        controller.Setup(actor, actorDependencyInjector, spawnData);
                    }
                })
                .AddTo(actor.GetCancellationTokenOnDestroy());
        }
    }
}
