using System.Collections;
using System.Collections.Generic;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private OpenCharacterController openCharacterController;

        public OpenCharacterController OpenCharacterController => this.openCharacterController;
    }
}
