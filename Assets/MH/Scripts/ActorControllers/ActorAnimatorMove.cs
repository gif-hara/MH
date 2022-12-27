using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorAnimatorMove : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        private Animator animator;
        
        private void Start()
        {
            this.animator = this.GetComponent<Animator>();
            Assert.IsNotNull(this.animator);
        }
        
        private void OnAnimatorMove()
        {
            this.actor.MoveController.Move(this.animator.deltaPosition);
        }
    }
}