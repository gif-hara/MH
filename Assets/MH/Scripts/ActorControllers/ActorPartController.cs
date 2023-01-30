using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の部位を制御するクラス
    /// </summary>
    public sealed class ActorPartController : IActorController
    {
        private Actor actor;

        private readonly Dictionary<int, ActorPartComponent> parts = new();

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
            this.actor = actor;
            foreach (var part in actor.GetComponentsInChildren<ActorPartComponent>())
            {
                this.parts.Add(part.gameObject.GetInstanceID(), part);
            }
        }

        public ActorPartComponent GetPart(GameObject partObject)
        {
            var result = this.parts[partObject.GetInstanceID()];
            Assert.IsNotNull(result, $"{partObject.name}に{typeof(ActorPartComponent)}が存在しません");

            return result;
        }

        public float GetDamageRate(GameObject partObject)
        {
            return this.actor.StatusController.GetPartDamageRate(this.GetPart(partObject).PartType);
        }
    }
}
