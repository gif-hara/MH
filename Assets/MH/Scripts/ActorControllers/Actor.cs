using System;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private ActorPostureController postureController;

        [SerializeField]
        private ActorAnimationController animationController;

        [SerializeField]
        private ActorModelController modelController;

        public ActorPostureController PostureController => this.postureController;
        
        public ActorAnimationController AnimationController => this.animationController;
        
        public ActorStateController StateController { private set; get; }
        
        public ActorDodgeController DodgeController { private set; get; }
        
        public ActorAttackController AttackController { private set; get; }

        public ActorModelController ModelController => this.modelController;
        
        public ActorTimeController TimeController { private set; get; }
        
        
        public Actor Spawn(ActorSpawnData data, Vector3 position, Quaternion rotation)
        {
            var instance = Instantiate(this, position, rotation);
            instance.StateController = new ActorStateController(instance);
            instance.DodgeController = new ActorDodgeController(instance);
            instance.AttackController = new ActorAttackController(instance, data.attackData);
            instance.TimeController = new ActorTimeController();

            foreach (var prefab in data.extensionPrefabs)
            {
                var extensionObject = Instantiate(prefab, instance.transform);
                foreach (var i in extensionObject.GetComponentsInChildren<IActorAttachable>())
                {
                    i.Attach(instance);
                }
            }
            return instance;
        }

        public Actor Spawn(ActorSpawnData data, Transform spawnPoint)
        {
            return Spawn(data, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
