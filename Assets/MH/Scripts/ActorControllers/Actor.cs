using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class Actor : MonoBehaviour, IActorDependencyInjector
    {

        [SerializeField]
        private AnimationController animationController;

        [SerializeField]
        private ModelDataHolder modelDataHolder;

        [SerializeField]
        private OpenCharacterController openCharacterController;

        public ActorPostureController PostureController { private set; get; }

        public ActorAnimationController AnimationController { private set; get; }

        public ActorModelController ModelController { private set; get; }

        public ActorStateController StateController { private set; get; }

        public ActorDodgeController DodgeController { private set; get; }

        public ActorAttackController AttackController { private set; get; }

        public ActorTimeController TimeController { private set; get; }

        public ActorStatusController StatusController { private set; get; }

        private void OnDestroy()
        {
            ActorManager.RemoveActor(this);
        }

        AnimationController IActorDependencyInjector.AnimationController => this.animationController;

        ModelDataHolder IActorDependencyInjector.ModelDataHolder => this.modelDataHolder;

        OpenCharacterController IActorDependencyInjector.OpenCharacterController => this.openCharacterController;

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
            this.PostureController = this.CreateActorController<ActorPostureController>(spawnData);
            this.TimeController = this.CreateActorController<ActorTimeController>(spawnData);
            this.ModelController = this.CreateActorController<ActorModelController>(spawnData);
            this.AnimationController = this.CreateActorController<ActorAnimationController>(spawnData);
            this.StateController = this.CreateActorController<ActorStateController>(spawnData);
            this.DodgeController = this.CreateActorController<ActorDodgeController>(spawnData);
            this.AttackController = this.CreateActorController<ActorAttackController>(spawnData);
            this.StatusController = this.CreateActorController<ActorStatusController>(spawnData);

            foreach (var prefab in spawnData.extensionPrefabs)
            {
                var extensionObject = Instantiate(prefab, this.transform);
                foreach (var i in extensionObject.GetComponentsInChildren<IActorController>())
                {
                    i.Setup(this, this, spawnData);
                }
            }

            ActorManager.AddActor(this);
        }

        private T CreateActorController<T>(ActorSpawnData spawnData) where T : IActorController, new()
        {
            var instance = new T();
            instance.Setup(this, this, spawnData);

            return instance;
        }
    }
}
