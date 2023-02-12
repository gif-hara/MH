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
            if (!actor.NetworkController.IsOwner)
            {
                return;
            }

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(this.waitSeconds));

                actor.NetworkController.NetworkBehaviour.SubmitRequestJudgeResult(this.result);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
