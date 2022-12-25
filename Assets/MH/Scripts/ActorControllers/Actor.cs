using System;
using System.Collections;
using System.Collections.Generic;
using Cookie;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private OpenCharacterController openCharacterController;

        [SerializeField]
        private AnimationController animationController;

        public ActorMoveController MoveController { private set; get; }
        
        public AnimationController AnimationController => this.animationController;

        private void Awake()
        {
            this.MoveController = new ActorMoveController(this.openCharacterController);
        }
    }
}
