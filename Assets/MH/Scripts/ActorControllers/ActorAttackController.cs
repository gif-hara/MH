using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorAttackController
    {
        private Actor actor;

        private ActorAttackData data;

        private int currentAttackIndex;

        private bool canRotate;

        private IDisposable invokeScope;

        public ActorAttackController(Actor actor, ActorAttackData data)
        {
            this.actor = actor;
            this.data = data;
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async UniTask InvokeAsync()
        {
            if (this.currentAttackIndex == -1)
            {
                return;
            }

            this.invokeScope?.Dispose();
            this.canRotate = false;
            var animationBlendData = this.data.motionDataList[this.currentAttackIndex].animationBlendData;
            
            // 次の攻撃を行うためインデックスを設定
            this.currentAttackIndex = this.data.motionDataList[this.currentAttackIndex].nextMotionIndex;

            var scope = DisposableBag.CreateBuilder();
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.RequestRotation>()
                .Subscribe(this.actor, x =>
                {
                    if (!this.canRotate)
                    {
                        return;
                    }
                    
                    this.actor.PostureController.Rotate(x.Rotation);
                })
                .AddTo(scope);
            
            MessageBroker.GetSubscriber<Actor, ActorEvents.AcceptRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.canRotate = true;
                })
                .AddTo(scope);

            MessageBroker.GetSubscriber<Actor, ActorEvents.CloseRequestRotation>()
                .Subscribe(this.actor, _ =>
                {
                    this.canRotate = false;
                })
                .AddTo(scope);

            this.invokeScope = scope.Build();

            await this.actor.AnimationController.PlayTask(animationBlendData);
            
            // 攻撃が終了したのでインデックスをリセット
            this.currentAttackIndex = 0;
            
            this.invokeScope?.Dispose();
            MessageBroker.GetPublisher<Actor, ActorEvents.EndAttack>()
                .Publish(this.actor, ActorEvents.EndAttack.Get());
        }
    }
}
