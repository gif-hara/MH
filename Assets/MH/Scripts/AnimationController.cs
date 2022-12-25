using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace MH
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
        
        private static readonly int OverrideStateA = Animator.StringToHash("Override State A");

        private static readonly int OverrideStateB = Animator.StringToHash("Override State B");

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
        /// 現在のアニメーションとブレンドしながら<see cref="clip"/>を再生する
        /// </summary>
        public void Play(AnimationClip clip, float blendSeconds = 0.0f)
        {
            // 前回のアニメーション処理を終了させる
            this.blendAnimationStream?.Dispose();
            this.updateAnimation.OnNext(CompleteType.Aborted);

            this.overrideClipToggle = !this.overrideClipToggle;
            this.overrideController[this.GetCurrentOverrideClipName()] = clip;
            this.animator.Play(this.GetCurrentOverrideState(), this.GetCurrentOverrideLayerIndex(), 0.0f);
            this.currentBlendSeconds = 0.0f;
            
            // ブレンドしない場合は即座にレイヤーを更新する
            if (blendSeconds <= 0.0f)
            {
                this.animator.SetLayerWeight(1, this.overrideClipToggle ? 1.0f : 0.0f);
                this.animator.SetLayerWeight(2, this.overrideClipToggle ? 0.0f : 1.0f);
            }
            this.animator.Update(0.0f);

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
        }

        public IObservable<CompleteType> PlayAsync(AnimationClip clip, float blendSeconds = 0.0f)
        {
            this.Play(clip, blendSeconds);
            
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
        
        private int GetCurrentOverrideState()
        {
            return this.overrideClipToggle ? OverrideStateA : OverrideStateB;
        }

        private int GetCurrentOverrideLayerIndex()
        {
            return this.overrideClipToggle ? 1 : 2;
        }
    }
}
