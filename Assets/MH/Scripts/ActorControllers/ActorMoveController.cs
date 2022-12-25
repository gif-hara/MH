using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorMoveController
    {
        private readonly OpenCharacterController openCharacterController;
        
        public ActorMoveController(OpenCharacterController openCharacterController)
        {
            this.openCharacterController = openCharacterController;
        }

        public void Move(Vector3 moveVector)
        {
            this.openCharacterController.Move(moveVector);
        }
    }
}
