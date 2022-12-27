using System;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Assertions;

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
        
        public ActorDodgeController DodgeController { private set; get; }
        
        public ActorAttackController AttackController { private set; get; }
        
        public Actor Spawn(ActorSpawnData data, Vector3 position, Quaternion rotation)
        {
            var instance = Instantiate(this, position, rotation);
            instance.StateController = new ActorStateController(instance);
            instance.DodgeController = new ActorDodgeController(instance);
            instance.AttackController = new ActorAttackController(instance, data.attackData);

            return instance;
        }

        public Actor Spawn(ActorSpawnData data, Transform spawnPoint)
        {
            return Spawn(data, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
