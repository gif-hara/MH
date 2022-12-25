using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;

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
        private AnimationClip idleClip;

        [SerializeField]
        private AnimationClip waveHandClip;

        [SerializeField]
        private float blendSeconds;

        private MHInputActions inputActions;

        private CinemachineOrbitalTransposer orbitalTransposer;

        private CinemachineComposer cinemachineComposer;

        private Vector3 lastRotation;
        
        private void Awake()
        {
            this.orbitalTransposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineOrbitalTransposer>();
            this.cinemachineComposer = this.cinemachineVirtualCamera.GetComponentInChildren<CinemachineComposer>();
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
            // キャラクターの移動処理
            var input = this.inputActions.Player.Move.ReadValue<Vector2>();
            var cameraTransform = this.cinemachineVirtualCamera.transform;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            var rightVelocity = input.x * cameraRight;
            var forwardVelocity = input.y * cameraForward;
            var velocity = (rightVelocity + forwardVelocity).normalized;
            this.actor.OpenCharacterController.Move(velocity * this.moveSpeed * Time.deltaTime);
            if (velocity.sqrMagnitude >= 0.01f)
            {
                this.lastRotation = velocity;
            }
            if (this.lastRotation.sqrMagnitude >= 0.01f)
            {
                this.actor.transform.localRotation =
                    Quaternion.Lerp(
                        this.actor.transform.localRotation,
                        Quaternion.LookRotation(this.lastRotation),
                        this.rotationSpeed * Time.deltaTime
                        );
            }

            // カメラのスクリーン値の更新
            var screenVelocity = input.x * this.screenMoveSpeed * Time.deltaTime;
            var screenX = Mathf.Clamp(
                this.cinemachineComposer.m_ScreenX + screenVelocity,
                this.screenXMin,
                this.screenXMax
                );
            this.cinemachineComposer.m_ScreenX = screenX;

            // カメラのY方向の回転処理
            input = this.inputActions.Player.Look.ReadValue<Vector2>();
            var offsetY = Mathf.Clamp(this.orbitalTransposer.m_FollowOffset.y + input.y * this.cameraSpeed.y * Time.deltaTime, this.followYMin, this.followYMax);
            this.orbitalTransposer.m_FollowOffset.y = offsetY;

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                this.actor.AnimationController.PlayBlend(this.idleClip, this.blendSeconds);
            }
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                this.actor.AnimationController.PlayBlend(this.waveHandClip, this.blendSeconds);
            }
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                this.actor.AnimationController.PlayAsync(this.waveHandClip)
                    .Subscribe(_ =>
                    {
                        Debug.Log(_);
                    });
            }
        }
    }
}
