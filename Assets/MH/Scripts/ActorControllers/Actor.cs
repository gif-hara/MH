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

        public OpenCharacterController OpenCharacterController => this.openCharacterController;

        public AnimationController AnimationController => this.animationController;
    }
}
