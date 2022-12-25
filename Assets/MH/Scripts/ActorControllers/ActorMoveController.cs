using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorMoveController : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;
        
        [SerializeField]
        private OpenCharacterController openCharacterController;

        private Vector3 currentMoveVector;

        private bool isMoving = true;

        private void LateUpdate()
        {
            if (this.currentMoveVector.sqrMagnitude > 0.0f)
            {
                if (!this.isMoving)
                {
                    this.isMoving = true;
                    MessageBroker.GetPublisher<Actor, ActorEvents.BeginMove>()
                        .Publish(this.actor, ActorEvents.BeginMove.Get());
                }

                this.openCharacterController.Move(this.currentMoveVector);
                this.currentMoveVector = Vector3.zero;
            }
            else
            {
                if (this.isMoving)
                {
                    this.isMoving = false;
                    MessageBroker.GetPublisher<Actor, ActorEvents.EndMove>()
                        .Publish(this.actor, ActorEvents.EndMove.Get());
                }
            }
        }

        public void Move(Vector3 moveVector)
        {
            this.currentMoveVector = moveVector;
        }
    }
}
