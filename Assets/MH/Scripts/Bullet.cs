using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public class Bullet : MonoBehaviour
    {
        private List<IGameObjectAction> actions;

        private void Update()
        {
            if (this.actions != null)
            {
                foreach (var action in this.actions)
                {
                    action.Invoke(this.gameObject);
                }
            }
        }

        public void SetActions(List<IGameObjectAction> actions)
        {
            this.actions = actions;
        }
    }
}
