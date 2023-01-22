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

        public float MoveSpeed => this.moveSpeed;

        public float RotationSpeed => this.rotationSpeed;

        public float DodgeSpeed => this.dodgeSpeed;

        public float DodgeDuration => this.dodgeDuration;

        public Ease DodgeEase => this.dodgeEase;

        public float AdvancedEntrySeconds => this.advancedEntrySeconds;
    }
}
