using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using MessagePipe;

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
        private float rotationSpeed;

        [SerializeField]
        private Vector2 cameraSpeed;

        [SerializeField]
        private float followYMax;

        [SerializeField]
        private float followYMin;

        [SerializeField]
        private float screenXMin;

        [SerializeField]
        private float screenXMax;

        [SerializeField]
        private float screenMoveSpeed;

        [SerializeField]
        private float dodgeSpeed;

        [SerializeField]
        private float dodgeDuration;

        [SerializeField]
        private Ease dodgeEase;
        
        private MHInputActions inputActions;

        private CinemachineOrbitalTransposer orbitalTransposer;

        private CinemachineComposer cinemachineComposer;

        private Vector3 lastRotation;

        private IDisposable disposable;
        
        private void Awake()
        {
            var bag = DisposableBag.CreateBuilder();
            MessageBroker.GetSubscriber<ActorEvents.SpawnedPlayer>()
                .Subscribe(x =>
                {
                    this.actor = x.Player;
                    this.cinemachineVirtualCamera.Follow = this.actor.transform;
                    this.cinemachineVirtualCamera.LookAt = this.actor.transform;
                    this.enabled = true;
                })
                .AddTo(bag);
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            this.orbitalTransposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineOrbitalTransposer>();
            this.cinemachineComposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineComposer>();

            this.inputActions = new MHInputActions();
            this.inputActions.Player.Dodge.performed += PerformedDodge;
            this.inputActions.Player.Fire.performed += PerformedFire;
            this.inputActions.Enable();
            this.enabled = false;

            this.disposable = bag.Build();
        }

        void Update()
        {
            // キャラクターの移動処理
            var deltaTime = this.actor.Time.deltaTime;
            var input = this.inputActions.Player.Move.ReadValue<Vector2>();
            var cameraTransform = this.cinemachineVirtualCamera.transform;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            var rightVelocity = input.x * cameraRight;
            var forwardVelocity = input.y * cameraForward;
            var velocity = (rightVelocity + forwardVelocity).normalized;
            if (velocity.sqrMagnitude >= 0.01f)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                    .Publish(this.actor, ActorEvents.RequestMove.Get(velocity * this.moveSpeed * deltaTime));
                this.lastRotation = velocity;
            }
            if (this.lastRotation.sqrMagnitude >= 0.01f)
            {
                var rotation = Quaternion.Lerp(
                    this.actor.transform.localRotation,
                    Quaternion.LookRotation(this.lastRotation),
                    this.rotationSpeed * deltaTime
                    );
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestRotation>()
                    .Publish(this.actor, ActorEvents.RequestRotation.Get(rotation));
            }

            // カメラのスクリーン値の更新
            var screenVelocity = input.x * this.screenMoveSpeed * deltaTime;
            var screenX = Mathf.Clamp(
                this.cinemachineComposer.m_ScreenX + screenVelocity,
                this.screenXMin,
                this.screenXMax
                );
            this.cinemachineComposer.m_ScreenX = screenX;

            // カメラのY方向の回転処理
            input = this.inputActions.Player.Look.ReadValue<Vector2>();
            var offsetY = Mathf.Clamp(this.orbitalTransposer.m_FollowOffset.y + input.y * this.cameraSpeed.y * deltaTime, this.followYMin, this.followYMax);
            this.orbitalTransposer.m_FollowOffset.y = offsetY;
        }

        private void OnDestroy()
        {
            this.disposable.Dispose();
        }

        private void PerformedDodge(InputAction.CallbackContext context)
        {
            var input = this.inputActions.Player.Move.ReadValue<Vector2>();
            var cameraTransform = this.cinemachineVirtualCamera.transform;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            var rightVelocity = input.x * cameraRight;
            var forwardVelocity = input.y * cameraForward;
            var direction = (rightVelocity + forwardVelocity).normalized;
            if (direction.sqrMagnitude <= 0.0f)
            {
                direction = Vector3.Scale(this.actor.transform.forward, new Vector3(1, 0, 1));
            }
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestDodge>()
                .Publish(this.actor, ActorEvents.RequestDodge.Get(
                    direction,
                    this.dodgeSpeed,
                    this.dodgeDuration,
                    this.dodgeEase
                    ));
        }
        
        private void PerformedFire(InputAction.CallbackContext obj)
        {
            MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttack>()
                .Publish(this.actor, ActorEvents.RequestAttack.Get());
        }
    }
}
