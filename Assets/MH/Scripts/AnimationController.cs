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
        
        private const string OverrideClipName = "Clip";

        private readonly Subject<Unit> updateAnimation = new();

        private void Awake()
        {
            this.overrideController = new AnimatorOverrideController();
            this.overrideController.runtimeAnimatorController = this.animator.runtimeAnimatorController;
            this.animator.runtimeAnimatorController = this.overrideController;
        }

        public void Play(AnimationClip clip)
        {
            this.PlayAsync(clip)
                .Subscribe();
        }

        public IObservable<Unit> PlayAsync(AnimationClip clip)
        {
            this.ChangeClip(clip);
            
            var completeStream = this.UpdateAsObservable()
                .TakeUntil(this.updateAnimation)
                .TakeUntilDestroy(this)
                .Where(_ => this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                .Take(1)
                .AsUnitObservable();

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
            while (this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }

        private void ChangeClip(AnimationClip clip)
        {
            this.overrideController[OverrideClipName] = clip;
            for (var i = 0; i < this.animator.layerCount; i++)
            {
                this.animator.Play(this.animator.GetCurrentAnimatorStateInfo(i).fullPathHash, i, 0.0f);
            }
            
            this.animator.Update(0.0f);
            this.updateAnimation.OnNext(Unit.Default);
        }
    }
}
