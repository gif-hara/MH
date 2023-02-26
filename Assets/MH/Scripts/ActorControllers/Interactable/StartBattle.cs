using System.Threading;
using Cysharp.Threading.Tasks;

namespace MH.ActorControllers.InteractableSystem
{
    /// <summary>
    /// バトルを開始する<see cref="ActorInteractable"/>
    /// </summary>
    public sealed class StartBattle : ActorInteractable
    {
        protected override UniTaskVoid OnBeginInteractAsync(Actor actor, CancellationToken cancellationToken)
        {
            if (actor.NetworkController.IsOwner)
            {
                actor.NetworkController.NetworkBehaviour.SubmitIsReadyBattle(true);
            }

            return new UniTaskVoid();
        }

        protected override void OnEndInteract(Actor actor)
        {
            if (actor.NetworkController.IsOwner)
            {
                actor.NetworkController.NetworkBehaviour.SubmitIsReadyBattle(false);
            }
        }
    }
}
