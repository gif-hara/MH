using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MH.ActorControllers.InteractableSystem;

namespace MH.ActorControllers
{
    /// <summary>
    /// 接触したオブジェクトから離れる処理を行うクラス
    /// </summary>
    public sealed class OnTriggerBeginInteract : IActorController
    {
        public void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            var ct = actor.GetCancellationTokenOnDestroy();
            actor.GetAsyncTriggerEnterTrigger()
                .Subscribe(x =>
                {
                    foreach (var actorInteractable in x.GetComponents<IActorInteractable>())
                    {
                        actorInteractable.BeginInteractAsync(actor);
                    }
                })
                .AddTo(ct);
            actor.GetAsyncTriggerExitTrigger()
                .Subscribe(x =>
                {
                    foreach (var actorInteractable in x.GetComponents<IActorInteractable>())
                    {
                        actorInteractable.EndInteract(actor);
                    }
                })
                .AddTo(ct);
        }
    }
}
