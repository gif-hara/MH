using Cysharp.Threading.Tasks;

namespace MH.ActorControllers.InteractableSystem
{
    /// <summary>
    /// <see cref="Actor"/>となにかしらインタラクトな処理を行うインターフェイス
    /// </summary>
    public interface IActorInteractable
    {
        UniTaskVoid BeginInteractAsync(Actor actor);

        void EndInteract(Actor actor);
    }
}
