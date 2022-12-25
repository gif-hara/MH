using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Cookie
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AnimationController : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        
        private AnimatorOverrideController overrideController;
        
        private bool overrideClipToggle = true;

        private readonly Subject<CompleteType> updateAnimation = new();

        private IDisposable blendAnimationStream;

        private float currentBlendSeconds;
        
        private const string OverrideClipAName = "ClipA";

        private const string OverrideClipBName = "ClipB";

        private const string OverrideStateAName = "Override State A";

        private const string OverrideStateBName = "Override State B";

        /// <summary>
        /// アニメーション再生完了タイプ
        /// </summary>
        public enum CompleteType
        {
            /// <summary>
            /// 最後まで再生した
            /// </summary>
            Success,
            
            /// <summary>
            /// 途中で終了した
            /// </summary>
            Aborted,
        }
        
        private void Awake()
        {
            this.overrideController = new AnimatorOverrideController();
            this.overrideController.runtimeAnimatorController = this.animator.runtimeAnimatorController;
            this.animator.runtimeAnimatorController = this.overrideController;
        }

        /// <summary>
        /// 単発のアニメーションを再生する
        /// </summary>
        public void PlaySingle(AnimationClip clip)
        {
            this.PlayAsync(clip)
                .Subscribe();
        }

        /// <summary>
        /// 現在のアニメーションとブレンドしながら<see cref="clip"/>を再生する
        /// </summary>
        public void PlayBlend(AnimationClip clip, float blendSeconds)
        {
            this.blendAnimationStream?.Dispose();
            this.overrideClipToggle = !this.overrideClipToggle;
            this.overrideController[this.GetCurrentOverrideClipName()] = clip;
            this.animator.Play(this.GetCurrentOverrideStateName(), this.GetCurrentOverrideLayerIndex(), 0.0f);
            this.animator.Update(0.0f);
            this.currentBlendSeconds = 0.0f;
            this.updateAnimation.OnNext(CompleteType.Aborted);

            if (blendSeconds > 0.0f)
            {
                this.blendAnimationStream = this.UpdateAsObservable()
                    .TakeUntilDestroy(this)
                    .Subscribe(_ =>
                    {
                        this.currentBlendSeconds = Mathf.Clamp01(this.currentBlendSeconds + Time.deltaTime);
                        var rate = this.currentBlendSeconds / blendSeconds;
                        this.animator.SetLayerWeight(1, this.overrideClipToggle ? rate : 1.0f - rate);
                        this.animator.SetLayerWeight(2, this.overrideClipToggle ? 1.0f - rate : rate);

                        if (rate >= 1.0f)
                        {
                            this.blendAnimationStream?.Dispose();
                        }
                    });
            }
            else
            {
                this.animator.SetLayerWeight(1, this.overrideClipToggle ? 1.0f : 0.0f);
                this.animator.SetLayerWeight(2, this.overrideClipToggle ? 0.0f : 1.0f);
            }
        }

        public IObservable<CompleteType> PlayAsync(AnimationClip clip)
        {
            this.PlayBlend(clip, 0.0f);
            
            var completeStream = this.UpdateAsObservable()
                .TakeUntil(this.updateAnimation)
                .TakeUntilDestroy(this)
                .Where(_ => this.animator.GetCurrentAnimatorStateInfo(this.GetCurrentOverrideLayerIndex()).normalizedTime >= 1.0f)
                .Take(1)
                .Select(_ => CompleteType.Success);

            return Observable.Merge(
                completeStream,
                this.updateAnimation
                )
                .Take(1)
                .TakeUntilDestroy(this);
        }

        public UniTask PlayTask(AnimationClip clip)
        {
            return this.PlayAsync(clip).ToUniTask();
        }

        public async UniTask WaitForAnimation()
        {
            while (this.animator.GetCurrentAnimatorStateInfo(this.GetCurrentOverrideLayerIndex()).normalizedTime < 1.0f)
            {
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }
        
        private string GetCurrentOverrideClipName()
        {
            return this.overrideClipToggle ? OverrideClipAName : OverrideClipBName;
        }
        
        private string GetCurrentOverrideStateName()
        {
            return this.overrideClipToggle ? OverrideStateAName : OverrideStateBName;
        }

        private int GetCurrentOverrideLayerIndex()
        {
            return this.overrideClipToggle ? 1 : 2;
        }
    }
}
