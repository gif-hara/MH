using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="IActorController.Setup"/>のタイミングでバトルの勝敗判定のリクエストを行うクラス
    /// </summary>
    public sealed class ActorSetupPublishRequestJudgeResult : MonoBehaviour, IActorController
    {
        [SerializeField]
        private Define.BattleResult result;

        [SerializeField]
        private float waitSeconds;

        public async void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(this.waitSeconds));

            MessageBroker.GetPublisher<BattleEvents.RequestJudgeResult>()
                .Publish(BattleEvents.RequestJudgeResult.Get(this.result));
        }
    }
}
