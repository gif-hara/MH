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

        public UniTaskVoid BeginInteractAsync()
        {
            Assert.IsNull(this.interactTokenSource, "Already Interacted.");
            this.interactTokenSource = new CancellationTokenSource();
            return this.OnBeginInteractAsync(this.interactTokenSource.Token);
        }

        public void EndInteract()
        {
            Assert.IsNotNull(this.interactTokenSource, "Not Interacting");
            this.OnEndInteract();
            this.interactTokenSource.Cancel();
            this.interactTokenSource.Dispose();
            this.interactTokenSource = null;
        }

        protected virtual UniTaskVoid OnBeginInteractAsync(CancellationToken cancellationToken)
        {
            return new UniTaskVoid();
        }

        protected virtual void OnEndInteract()
        {
        }
    }
}
