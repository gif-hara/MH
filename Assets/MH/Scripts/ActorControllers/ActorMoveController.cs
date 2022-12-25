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
        private OpenCharacterController openCharacterController;

        private Vector3 currentMoveVector;

        private bool isMoving;

        private void LateUpdate()
        {
            if (this.currentMoveVector.sqrMagnitude > 0.0f)
            {
                if (!this.isMoving)
                {
                    this.isMoving = true;
                    MessageBroker.GetPublisher<ActorEvents.BeginMove>()
                        .Publish(ActorEvents.BeginMove.Get());
                }

                this.openCharacterController.Move(this.currentMoveVector);
                this.currentMoveVector = Vector3.zero;
            }
            else
            {
                if (this.isMoving)
                {
                    this.isMoving = false;
                    MessageBroker.GetPublisher<ActorEvents.EndMove>()
                        .Publish(ActorEvents.EndMove.Get());
                }
            }
        }

        public void Move(Vector3 moveVector)
        {
            this.currentMoveVector = moveVector;
        }
    }
}
