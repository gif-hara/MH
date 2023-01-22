using System;
using Cinemachine;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MH
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private CinemachineVirtualCamera cinemachineVirtualCamera;

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

        /// <summary>
        /// 先行入力を行っている処理
        /// </summary>
        private readonly DisposableBagBuilder advancedEntryScope = DisposableBag.CreateBuilder();

        private CinemachineComposer cinemachineComposer;

        private MHInputActions inputActions;

        private Vector3 lastRotation;

        private CinemachineOrbitalTransposer orbitalTransposer;

        private PlayerActorCommonData playerActorCommonData;

        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            // キャラクターの移動処理
            var deltaTime = actor.TimeController.Time.deltaTime;
            var input = inputActions.Player.Move.ReadValue<Vector2>();
            var cameraTransform = cinemachineVirtualCamera.transform;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            var rightVelocity = input.x * cameraRight;
            var forwardVelocity = input.y * cameraForward;
            var velocity = (rightVelocity + forwardVelocity).normalized;
            if (velocity.sqrMagnitude >= 0.01f)
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestMove>()
                    .Publish(actor, ActorEvents.RequestMove.Get(velocity * this.playerActorCommonData.MoveSpeed * deltaTime));
                lastRotation = velocity;
            }
            if (lastRotation.sqrMagnitude >= 0.01f)
            {
                var rotation = Quaternion.Lerp(
                    actor.transform.localRotation,
                    Quaternion.LookRotation(lastRotation),
                    this.playerActorCommonData.RotationSpeed * deltaTime
                    );
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestRotation>()
                    .Publish(actor, ActorEvents.RequestRotation.Get(rotation));
            }

            // カメラのスクリーン値の更新
            var screenVelocity = input.x * screenMoveSpeed * deltaTime;
            var screenX = Mathf.Clamp(
                cinemachineComposer.m_ScreenX + screenVelocity,
                screenXMin,
                screenXMax
                );
            cinemachineComposer.m_ScreenX = screenX;

            // カメラのY方向の回転処理
            input = inputActions.Player.Look.ReadValue<Vector2>();
            var offsetY = Mathf.Clamp(orbitalTransposer.m_FollowOffset.y + input.y * cameraSpeed.y * deltaTime, followYMin, followYMax);
            orbitalTransposer.m_FollowOffset.y = offsetY;
        }

        public void Attach(Actor actor, PlayerActorCommonData playerActorCommonData)
        {
            this.actor = actor;
            this.playerActorCommonData = playerActorCommonData;
            var t = actor.transform;
            cinemachineVirtualCamera.Follow = t;
            cinemachineVirtualCamera.LookAt = t;
            enabled = true;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            orbitalTransposer = cinemachineVirtualCamera.GetComponentInChildren<CinemachineOrbitalTransposer>();
            cinemachineComposer = cinemachineVirtualCamera.GetComponentInChildren<CinemachineComposer>();

            inputActions = new MHInputActions();
            inputActions.Player.Dodge.performed += PerformedDodge;
            inputActions.Player.AttackWeak.performed += PerformedAttackWeak;
            inputActions.Player.AttackStrong.performed += PerformedAttackStrong;
            inputActions.Enable();
        }

        private void PerformedDodge(InputAction.CallbackContext context)
        {
            this.RegisterAdvancedEntry(() =>
            {
                var input = inputActions.Player.Move.ReadValue<Vector2>();
                var cameraTransform = cinemachineVirtualCamera.transform;
                var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
                var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
                var rightVelocity = input.x * cameraRight;
                var forwardVelocity = input.y * cameraForward;
                var direction = (rightVelocity + forwardVelocity).normalized;
                if (direction.sqrMagnitude <= 0.0f)
                {
                    direction = Vector3.Scale(actor.transform.forward, new Vector3(1, 0, 1));
                }
                var invokeData = new ActorDodgeController.InvokeData
                {
                    direction = direction,
                    speed = this.playerActorCommonData.DodgeSpeed,
                    duration = this.playerActorCommonData.DodgeDuration,
                    ease = this.playerActorCommonData.DodgeEase
                };
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestDodge>()
                    .Publish(actor, ActorEvents.RequestDodge.Get(invokeData));
            });
        }

        private void PerformedAttackWeak(InputAction.CallbackContext obj)
        {
            this.RegisterAdvancedEntry(() =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttack>()
                    .Publish(actor, ActorEvents.RequestAttack.Get(Define.RequestAttackType.Weak));
            });
        }

        private void PerformedAttackStrong(InputAction.CallbackContext obj)
        {
            this.RegisterAdvancedEntry(() =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttack>()
                    .Publish(actor, ActorEvents.RequestAttack.Get(Define.RequestAttackType.Strong));
            });
        }

        private void RegisterAdvancedEntry(Action entryAction)
        {
            entryAction();

            this.advancedEntryScope.Clear();
            this.GetAsyncUpdateTrigger()
                .Subscribe(_ => entryAction())
                .AddTo(this.advancedEntryScope);
            UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(this.playerActorCommonData.AdvancedEntrySeconds))
                .Subscribe(_ =>
                {
                    this.advancedEntryScope.Clear();
                })
                .AddTo(this.advancedEntryScope);
        }
    }
}
