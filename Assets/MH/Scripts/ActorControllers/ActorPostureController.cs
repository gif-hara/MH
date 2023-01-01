using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の姿勢を制御するクラス
    /// </summary>
    public sealed class ActorPostureController : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;
        
        [SerializeField]
        private OpenCharacterController openCharacterController;

        [SerializeField]
        private Vector3 gravity;

        private Vector3 currentGravity;

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
            
            this.currentGravity += this.gravity * this.actor.TimeController.Time.deltaTime;

            var totalVector = this.currentMoveVector + this.currentGravity;

            this.openCharacterController.Move(totalVector);
            this.currentMoveVector = Vector3.zero;

            if (this.openCharacterController.isGrounded)
            {
                this.currentGravity = Vector3.zero;
            }
        }

        public void Move(Vector3 moveVector)
        {
            this.currentMoveVector += moveVector;
        }

        public void Rotate(Quaternion rotation)
        {
            this.actor.transform.localRotation = rotation;
        }
    }
}
