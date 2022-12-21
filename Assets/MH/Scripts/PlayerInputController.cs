using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace MH
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private CinemachineVirtualCamera cinemachineVirtualCamera;

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private Vector2 cameraSpeed;

        [SerializeField]
        private float cameraYMax;

        [SerializeField]
        private float cameraYMin;

        private MHInputActions inputActions;

        private CinemachineOrbitalTransposer orbitalTransposer;

        private void Awake()
        {
            this.orbitalTransposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineOrbitalTransposer>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Start()
        {
            this.inputActions = new MHInputActions();
            this.inputActions.Enable();
        }

        void Update()
        {
            var input = this.inputActions.Player.Move.ReadValue<Vector2>();
            var t = this.actor.transform;
            var rightVelocity = input.x * t.right;
            var forwardVelocity = input.y * t.forward;
            this.actor.OpenCharacterController.Move((rightVelocity + forwardVelocity).normalized * this.moveSpeed * Time.deltaTime);

            input = this.inputActions.Player.Look.ReadValue<Vector2>();
            var vector = new Vector3(
                input.x * this.cameraSpeed.x,
                input.y * this.cameraSpeed.y,
                0.0f
                );
            var cameraY = Mathf.Clamp(this.orbitalTransposer.m_FollowOffset.y + vector.y * Time.deltaTime, this.cameraYMin, this.cameraYMax);
            this.orbitalTransposer.m_FollowOffset.y = cameraY;
            this.actor.transform.localRotation *= Quaternion.Euler(0.0f, vector.x * Time.deltaTime, 0.0f);

            if (this.inputActions.Player.Fire.IsPressed())
            {
                this.actor.MuzzleController.Fire();
            }
        }
    }
}
