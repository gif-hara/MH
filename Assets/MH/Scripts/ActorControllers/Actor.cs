using UnityEngine;

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

        public ActorModelController ModelController => this.modelController;

        public ActorStateController StateController { private set; get; }

        public ActorDodgeController DodgeController { private set; get; }

        public ActorAttackController AttackController { private set; get; }

        public ActorTimeController TimeController { private set; get; }

        public Actor Spawn(ActorSpawnData data, Vector3 position, Quaternion rotation)
        {
            var instance = Instantiate(this, position, rotation);
            instance.Initialize(data);
            return instance;
        }

        public Actor Spawn(ActorSpawnData data, Transform spawnPoint)
        {
            return Spawn(data, spawnPoint.position, spawnPoint.rotation);
        }

        private void Initialize(ActorSpawnData spawnData)
        {
            this.StateController = this.CreateActorController<ActorStateController>(spawnData);
            this.DodgeController = this.CreateActorController<ActorDodgeController>(spawnData);
            this.AttackController = this.CreateActorController<ActorAttackController>(spawnData);
            this.TimeController = this.CreateActorController<ActorTimeController>(spawnData);

            foreach (var prefab in spawnData.extensionPrefabs)
            {
                var extensionObject = Instantiate(prefab, this.transform);
                foreach (var i in extensionObject.GetComponentsInChildren<IActorController>())
                {
                    i.Setup(this, spawnData);
                }
            }
        }

        private T CreateActorController<T>(ActorSpawnData spawnData) where T : IActorController, new()
        {
            var instance = new T();
            instance.Setup(this, spawnData);

            return instance;
        }
    }
}
