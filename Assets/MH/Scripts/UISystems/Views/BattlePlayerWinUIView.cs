using UnityEngine;

namespace MH.UISystems
{
    /// <summary>
    /// バトルシーンプレイヤーが勝利した際の<see cref="UIView"/>
    /// </summary>
    public sealed class BattlePlayerWinUIView : UIView
    {
        [SerializeField]
        private AnimationController animationController;

        [SerializeField]
        private AnimationClip inAnimation;

        [SerializeField]
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            this.canvasGroup.alpha = 0.0f;
        }

        public void PlayInAnimation()
        {
            this.animationController.Play(this.inAnimation);
        }
    }
}
