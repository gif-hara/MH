using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorAttachable
    {
        [SerializeField]
        private List<Collider> colliders;
        
        private Actor actor;

        private Dictionary<string, Collider> colliderDictionary;

        public void Attach(Actor actor)
        {
            this.actor = actor;
            var t = this.transform;
            t.SetParent(this.actor.ModelController.BoneHolder.RightHand, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            this.colliderDictionary = this.colliders.ToDictionary(x => x.name);
        }
    }
}
