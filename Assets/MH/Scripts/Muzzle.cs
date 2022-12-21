using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public class Muzzle : MonoBehaviour
    {
        [SerializeField]
        private Bullet bulletPrefab;

        [SerializeReference, SubclassSelector(typeof(IGameObjectAction))]
        private List<IGameObjectAction> actions;

        public void Fire()
        {
            var t = this.transform;
            var bullet = Instantiate(this.bulletPrefab, t.position, t.rotation);
            bullet.SetActions(this.actions);
        }
    }
}
