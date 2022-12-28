using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の武器を制御するクラス
    /// </summary>
    public sealed class ActorWeaponController : MonoBehaviour, IActorAttachable
    {
        private Actor actor;
        
        public void Attach(Actor actor)
        {
            this.actor = actor;
            var t = this.transform;
            t.SetParent(this.actor.ModelController.BoneHolder.RightHand, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
    }
}
