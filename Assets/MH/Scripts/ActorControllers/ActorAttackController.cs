using Cysharp.Threading.Tasks;

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

        public ActorAttackController(Actor actor, ActorAttackData data)
        {
            this.actor = actor;
            this.data = data;
        }

        /// <summary>
        /// 攻撃を実行する
        /// </summary>
        public async UniTask Invoke()
        {
            if (this.currentAttackIndex == -1)
            {
                return;
            }

            var animationBlendData = this.data.motionDataList[this.currentAttackIndex].animationBlendData;
            
            // 次の攻撃を行うためインデックスを設定
            this.currentAttackIndex = this.data.motionDataList[this.currentAttackIndex].nextMotionIndex;
            
            await this.actor.AnimationController.PlayTask(animationBlendData);
            
            // 攻撃が終了したのでインデックスをリセット
            this.currentAttackIndex = 0;
            
            MessageBroker.GetPublisher<Actor, ActorEvents.EndAttack>()
                .Publish(this.actor, ActorEvents.EndAttack.Get());
        }
    }
}
