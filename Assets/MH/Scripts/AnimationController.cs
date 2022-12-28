using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
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
        
        private int currentLayerIndex = LayerAIndex;

        private CancellationTokenDisposable animationCancelToken;

        private float currentBlendSeconds;
        
        private const string OverrideClipAName = "ClipA";

        private const string OverrideClipBName = "ClipB";
        
        private static readonly int OverrideStateA = Animator.StringToHash("Override State A");

        private static readonly int OverrideStateB = Animator.StringToHash("Override State B");

        private const int LayerAIndex = 1;

        private const int LayerBIndex = 2;

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

        private void OnDestroy()
        {
            this.animationCancelToken?.Dispose();
        }

        /// <summary>
        /// 現在のアニメーションとブレンドしながら<see cref="clip"/>を再生する
        /// </summary>
        public void Play(AnimationClip clip, float blendSeconds = 0.0f)
        {
            // 前回のアニメーション処理を終了させる
            this.animationCancelToken?.Dispose();
            this.animationCancelToken = new CancellationTokenDisposable();

            this.currentLayerIndex = this.currentLayerIndex == LayerAIndex ? LayerBIndex : LayerAIndex;
            this.overrideController[this.GetCurrentOverrideClipName()] = clip;
            this.animator.Play(this.GetCurrentOverrideState(), this.currentLayerIndex, 0.0f);
            this.currentBlendSeconds = 0.0f;
            
            // ブレンドしない場合は即座にレイヤーを更新する
            if (blendSeconds <= 0.0f)
            {
                this.animator.SetLayerWeight(LayerAIndex, this.currentLayerIndex == LayerAIndex ? 1.0f : 0.0f);
                this.animator.SetLayerWeight(LayerBIndex, this.currentLayerIndex == LayerBIndex ? 1.0f : 0.0f);
            }
            this.animator.Update(0.0f);

            if (blendSeconds > 0.0f)
            {
                this.StartBlendAsync(blendSeconds, this.animationCancelToken.Token).Forget();
            }
        }

        public void Play(AnimationBlendData data)
        {
            this.Play(data.animationClip, data.blendSeconds);
        }
        
        public UniTask<CompleteType> PlayTask(AnimationClip clip, float blendSeconds = 0.0f)
        {
            this.Play(clip, blendSeconds);
            return this.GetCompleteAnimationTask(this.animationCancelToken.Token);
        }

        public UniTask<CompleteType> PlayTask(AnimationBlendData blendData)
        {
            return this.PlayTask(blendData.animationClip, blendData.blendSeconds);
        }

        private async UniTask<CompleteType> GetCompleteAnimationTask(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return CompleteType.Aborted;
            }
            
            while (this.animator.GetCurrentAnimatorStateInfo(this.currentLayerIndex).normalizedTime < 1.0f)
            {
                if (token.IsCancellationRequested)
                {
                    return CompleteType.Aborted;
                }
                
                await UniTask.NextFrame(PlayerLoopTiming.Update, token);
            }

            return CompleteType.Success;
        }
        
        private async UniTask StartBlendAsync(float blendSeconds, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            
            this.currentBlendSeconds += Time.deltaTime;
            var rate = this.currentBlendSeconds / blendSeconds;
            while (rate < 1.0f)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                
                this.animator.SetLayerWeight(LayerAIndex, this.currentLayerIndex == LayerAIndex ? rate : 1.0f - rate);
                this.animator.SetLayerWeight(LayerBIndex, this.currentLayerIndex == LayerBIndex ? rate : 1.0f - rate);
                await UniTask.NextFrame(PlayerLoopTiming.Update, token);
                this.currentBlendSeconds += Time.deltaTime;
                rate = this.currentBlendSeconds / blendSeconds;
            }
        }

        public async UniTask WaitForAnimation()
        {
            while (this.animator.GetCurrentAnimatorStateInfo(this.currentLayerIndex).normalizedTime < 1.0f)
            {
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }
        
        private string GetCurrentOverrideClipName()
        {
            return this.currentLayerIndex == LayerAIndex ? OverrideClipAName : OverrideClipBName;
        }
        
        private int GetCurrentOverrideState()
        {
            return this.currentLayerIndex == LayerAIndex ? OverrideStateA : OverrideStateB;
        }
    }
}
