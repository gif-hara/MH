using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="IActorController.Setup"/>のタイミングで指定したモデルにアタッチするクラス
    /// </summary>
    public sealed class SetupActorModelAttach : MonoBehaviour, IActorController
    {
        [SerializeField]
        private ModelDataHolder.PartType partType;

        public void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            var t = transform;
            t.SetParent(
                actor.ModelController.ModelDataHolder.GetPart(this.partType),
                false
                );
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
    }
}
