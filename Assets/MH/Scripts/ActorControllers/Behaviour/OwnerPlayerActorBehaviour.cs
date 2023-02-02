using System;
using System.Text;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using MH.NetworkSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MH.ActorControllers
{
    /// <summary>
    /// プレイヤーが操作する<see cref="Actor"/>の制御を行うクラス
    /// </summary>
    public sealed class OwnerPlayerActorBehaviour
    {
        /// <summary>
        /// 先行入力を行っている処理
        /// </summary>
        private readonly DisposableBagBuilder advancedEntryScope = DisposableBag.CreateBuilder();

        private PlayerNetworkBehaviour playerNetworkBehaviour;

        private Actor actor;

        private CinemachineComposer cinemachineComposer;

        private CinemachineVirtualCamera cinemachineVirtualCamera;

        private Vector3 lastRotation;

        private CinemachineOrbitalTransposer orbitalTransposer;

        public static void Attach(Actor actor, PlayerNetworkBehaviour playerNetworkBehaviour)
        {
            var instance = new OwnerPlayerActorBehaviour();
            instance._Attach(actor, playerNetworkBehaviour);
        }

        private OwnerPlayerActorBehaviour()
        {
        }

        private void _Attach(Actor actor, PlayerNetworkBehaviour playerNetworkBehaviour)
        {
            this.actor = actor;
            this.playerNetworkBehaviour = playerNetworkBehaviour;
            this.cinemachineVirtualCamera = CameraController.Instance.Player;
            var t = this.actor.transform;
            this.cinemachineVirtualCamera.Follow = t;
            this.cinemachineVirtualCamera.LookAt = t;
            this.orbitalTransposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineOrbitalTransposer>();
            this.cinemachineComposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineComposer>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            var playerActorCommonData = PlayerActorCommonData.Instance;
            var inputActions = InputController.InputActions;
            inputActions.Player.Dodge.performed += PerformedDodge;
            inputActions.Player.AttackWeak.performed += PerformedAttackWeak;
            inputActions.Player.AttackStrong.performed += PerformedAttackStrong;
            inputActions.Enable();

            var ct = this.actor.GetCancellationTokenOnDestroy();
            actor.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    // キャラクターの移動処理
                    var deltaTime = actor.TimeController.Time.deltaTime;
                    var input = InputController.InputActions.Player.Move.ReadValue<Vector2>();
                    var cameraTransform = this.cinemachineVirtualCamera.transform;
                    var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
                    var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
                    var rightVelocity = input.x * cameraRight;
                    var forwardVelocity = input.y * cameraForward;
                    var velocity = (rightVelocity + forwardVelocity).normalized;
                    if (velocity.sqrMagnitude >= 0.01f)
                    {
                        this.actor.PostureController.Move(velocity * playerActorCommonData.MoveSpeed * deltaTime);
                        lastRotation = velocity;
                    }
                    if (lastRotation.sqrMagnitude >= 0.01f)
                    {
                        var rotation = Quaternion.Lerp(
                            actor.transform.localRotation,
                            Quaternion.LookRotation(lastRotation),
                            playerActorCommonData.RotationSpeed * deltaTime
                            );
                        this.actor.PostureController.Rotate(rotation);
                    }

                    // カメラのスクリーン値の更新
                    var screenVelocity = input.x * playerActorCommonData.ScreenMoveSpeed * deltaTime;
                    var screenX = Mathf.Clamp(
                        cinemachineComposer.m_ScreenX + screenVelocity,
                        playerActorCommonData.ScreenXMin,
                        playerActorCommonData.ScreenXMax
                        );
                    cinemachineComposer.m_ScreenX = screenX;

                    // カメラのY方向の回転処理
                    input = InputController.InputActions.Player.Look.ReadValue<Vector2>();
                    var offsetY = Mathf.Clamp(
                        orbitalTransposer.m_FollowOffset.y + input.y * playerActorCommonData.CameraMoveSpeed.y * deltaTime,
                        playerActorCommonData.FollowYMin,
                        playerActorCommonData.FollowYMax
                        );
                    orbitalTransposer.m_FollowOffset.y = offsetY;

                    // ガード処理
                    var isGuard = inputActions.Player.Guard.ReadValue<float>() > 0.5f;
                    if (isGuard)
                    {
                        this.actor.GuardController.Begin();
                    }
                    else
                    {
                        this.actor.GuardController.End();
                    }
                })
                .AddTo(ct);

            UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(0.1f))
                .Subscribe(_ =>
                {
                    this.TrySubmitPosition();
                    this.TrySubmitRotation();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginAttack>()
                .Subscribe(this.actor, x =>
                {
                    this.playerNetworkBehaviour.SubmitRequestUniqueMotion(x.MotionName);
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.playerNetworkBehaviour.SubmitBeginDodge(x.Data);
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.GaveDamage>()
                .Subscribe(this.actor, x =>
                {
                    this.playerNetworkBehaviour.SubmitGaveDamage(
                        x.Data.receiveActor.NetworkController.NetworkObjectId,
                        x.Data.damage,
                        x.Data.partType
                        );
                })
                .AddTo(ct);

#if MH_DEBUG
            this.actor.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(this.actor.name);
                    builder.AppendLine($"  Guarding = {this.actor.GuardController.Guarding}");
                    MessageBroker.GetPublisher<DebugPanelEvents.AppendLine>()
                        .Publish(DebugPanelEvents.AppendLine.Get(builder.ToString()));
                })
                .AddTo(ct);
#endif
        }

        private void PerformedDodge(InputAction.CallbackContext context)
        {
            this.RegisterAdvancedEntry(() =>
            {
                var playerActorCommonData = PlayerActorCommonData.Instance;
                var input = InputController.InputActions.Player.Move.ReadValue<Vector2>();
                var cameraTransform = cinemachineVirtualCamera.transform;
                var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
                var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
                var rightVelocity = input.x * cameraRight;
                var forwardVelocity = input.y * cameraForward;
                var direction = (rightVelocity + forwardVelocity).normalized;
                if (direction.sqrMagnitude <= 0.0f)
                {
                    direction = Vector3.Scale(this.actor.transform.forward, new Vector3(1, 0, 1));
                }
                var invokeData = new ActorDodgeController.InvokeData
                {
                    direction = direction,
                    speed = playerActorCommonData.DodgeSpeed,
                    duration = playerActorCommonData.DodgeDuration,
                    ease = playerActorCommonData.DodgeEase
                };
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestDodge>()
                    .Publish(this.actor, ActorEvents.RequestDodge.Get(invokeData, false));
            });
        }

        private void PerformedAttackWeak(InputAction.CallbackContext obj)
        {
            this.RegisterAdvancedEntry(() =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttack>()
                    .Publish(this.actor, ActorEvents.RequestAttack.Get(Define.RequestAttackType.Weak));
            });
        }

        private void PerformedAttackStrong(InputAction.CallbackContext obj)
        {
            this.RegisterAdvancedEntry(() =>
            {
                MessageBroker.GetPublisher<Actor, ActorEvents.RequestAttack>()
                    .Publish(this.actor, ActorEvents.RequestAttack.Get(Define.RequestAttackType.Strong));
            });
        }

        private void RegisterAdvancedEntry(Action entryAction)
        {
            entryAction();

            var playerActorCommonData = PlayerActorCommonData.Instance;
            this.advancedEntryScope.Clear();
            this.actor.GetAsyncUpdateTrigger()
                .Subscribe(_ => entryAction())
                .AddTo(this.advancedEntryScope);
            UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(playerActorCommonData.AdvancedEntrySeconds))
                .Subscribe(_ =>
                {
                    this.advancedEntryScope.Clear();
                })
                .AddTo(this.advancedEntryScope);
        }

        private void TrySubmitPosition()
        {
            var playerActorCommonData = PlayerActorCommonData.Instance;
            var distance = (this.actor.transform.localPosition - this.playerNetworkBehaviour.NetworkPosition).sqrMagnitude;
            if (distance > playerActorCommonData.SendPositionThreshold * playerActorCommonData.SendPositionThreshold)
            {
                this.playerNetworkBehaviour.SubmitPosition(this.actor.transform.localPosition);
            }
        }

        private void TrySubmitRotation()
        {
            var playerActorCommonData = PlayerActorCommonData.Instance;
            var difference = this.playerNetworkBehaviour.NetworkRotation - this.actor.transform.localRotation.eulerAngles.y;
            difference = difference < 0 ? -difference : difference;
            if (difference > playerActorCommonData.SendRotationThreshold)
            {
                this.playerNetworkBehaviour.SubmitRotation(this.actor.transform.localRotation.eulerAngles.y);
            }
        }
    }
}
