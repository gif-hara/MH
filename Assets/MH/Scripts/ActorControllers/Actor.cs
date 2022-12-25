using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private ActorMoveController moveController;

        [SerializeField]
        private ActorAnimationController animationController;

        public ActorMoveController MoveController => this.moveController;
        
        public ActorAnimationController AnimationController => this.animationController;
        
        public ActorStateController StateController { private set; get; }

        private void Awake()
        {
            this.StateController = new ActorStateController(this);
        }
    }
}
