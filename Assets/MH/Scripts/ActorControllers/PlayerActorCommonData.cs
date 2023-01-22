using DG.Tweening;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// プレイヤーが操作する<see cref="Actor"/>の基本データを持つクラス
    /// </summary>
    [CreateAssetMenu(menuName = "MH/PlayerActorCommonData")]
    public sealed class PlayerActorCommonData : ScriptableObject
    {
        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float rotationSpeed;

        [SerializeField]
        private float dodgeSpeed;

        [SerializeField]
        private float dodgeDuration;

        [SerializeField]
        private Ease dodgeEase;

        /// <summary>
        /// 先行入力を行う秒数
        /// </summary>
        [SerializeField]
        private float advancedEntrySeconds;

        /// <summary>
        /// カメラの移動速度
        /// </summary>
        [SerializeField]
        private Vector2 cameraSpeed;

        /// <summary>
        /// カメラのフォローYの最小値
        /// </summary>
        [SerializeField]
        private float followYMin;

        /// <summary>
        /// カメラのフォローYの最大値
        /// </summary>
        [SerializeField]
        private float followYMax;

        /// <summary>
        /// カメラのスクリーンXの最小値
        /// </summary>
        [SerializeField]
        private float screenXMin;

        /// <summary>
        /// カメラのスクリーンXの最大値
        /// </summary>
        [SerializeField]
        private float screenXMax;

        /// <summary>
        /// カメラのスクリーンの移動速度
        /// </summary>
        [SerializeField]
        private float screenMoveSpeed;

        public float MoveSpeed => this.moveSpeed;

        public float RotationSpeed => this.rotationSpeed;

        public float DodgeSpeed => this.dodgeSpeed;

        public float DodgeDuration => this.dodgeDuration;

        public Ease DodgeEase => this.dodgeEase;

        public float AdvancedEntrySeconds => this.advancedEntrySeconds;

        public Vector2 CameraMoveSpeed => this.cameraSpeed;

        public float FollowYMin => this.followYMin;

        public float FollowYMax => this.followYMax;

        public float ScreenXMin => this.screenXMin;

        public float ScreenXMax => this.screenXMax;

        public float ScreenMoveSpeed => this.screenMoveSpeed;
    }
}
