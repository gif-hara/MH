using Cookie;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private OpenCharacterController openCharacterController;

        [SerializeField]
        private AnimationController animationController;

        public ActorMoveController MoveController { private set; get; }
        
        public AnimationController AnimationController => this.animationController;
        
        public ActorStateController StateController { private set; get; }

        private void Awake()
        {
            this.MoveController = new ActorMoveController(this.openCharacterController);
            this.StateController = new ActorStateController(this);
        }
    }
}
