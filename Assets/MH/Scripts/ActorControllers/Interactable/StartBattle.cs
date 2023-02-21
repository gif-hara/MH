using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MH.ActorControllers.InteractableSystem
{
    /// <summary>
    /// バトルを開始する<see cref="ActorInteractable"/>
    /// </summary>
    public sealed class StartBattle : ActorInteractable
    {
        [SerializeField]
        private float waitSeconds;

        protected override async UniTaskVoid OnBeginInteractAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log("Wait StartBattle");

                await UniTask.Delay(TimeSpan.FromSeconds(this.waitSeconds), cancellationToken: cancellationToken);
                MessageBroker.GetPublisher<BattleEvents.RequestBeginBattle>()
                    .Publish(BattleEvents.RequestBeginBattle.Get());
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
