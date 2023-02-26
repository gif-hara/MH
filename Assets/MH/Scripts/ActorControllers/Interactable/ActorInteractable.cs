using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers.InteractableSystem
{
    /// <summary>
    /// <see cref="Actor"/>とインタラクト可能な<see cref="MonoBehaviour"/>
    /// </summary>
    public abstract class ActorInteractable : MonoBehaviour, IActorInteractable
    {
        private CancellationTokenSource interactTokenSource;

        public UniTaskVoid BeginInteractAsync(Actor actor)
        {
            Assert.IsNull(this.interactTokenSource, "Already Interacted.");
            this.interactTokenSource = new CancellationTokenSource();
            return this.OnBeginInteractAsync(actor, this.interactTokenSource.Token);
        }

        public void EndInteract(Actor actor)
        {
            Assert.IsNotNull(this.interactTokenSource, "Not Interacting");
            this.OnEndInteract(actor);
            this.interactTokenSource.Cancel();
            this.interactTokenSource.Dispose();
            this.interactTokenSource = null;
        }

        protected virtual UniTaskVoid OnBeginInteractAsync(Actor actor, CancellationToken cancellationToken)
        {
            return new UniTaskVoid();
        }

        protected virtual void OnEndInteract(Actor actor)
        {
        }
    }
}
