using UnityEngine;

namespace MH.UISystems
{
    /// <summary>
    /// バトルの勝敗判定を表示する<see cref="UIView"/>
    /// </summary>
    public sealed class BattleJudgementUIView : UIView
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
