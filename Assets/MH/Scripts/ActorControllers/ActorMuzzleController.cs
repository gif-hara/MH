using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public class ActorMuzzleController
    {
        private List<Muzzle> muzzles;
        
        public ActorMuzzleController(Actor actor)
        {
            this.muzzles = new List<Muzzle>(actor.gameObject.GetComponentsInChildren<Muzzle>());
        }

        public void Fire()
        {
            foreach (var muzzle in this.muzzles)
            {
                muzzle.Fire();
            }
        }
    }
}
