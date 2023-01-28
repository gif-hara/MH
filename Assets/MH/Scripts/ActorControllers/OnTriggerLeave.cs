using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// 接触したオブジェクトから離れる処理を行うクラス
    /// </summary>
    public sealed class OnTriggerLeave : IActorController
    {
        public void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            var leaveDistance = spawnData.leaveDistance;
            var ct = actor.GetCancellationTokenOnDestroy();
            actor.GetAsyncTriggerStayTrigger()
                .Subscribe(x =>
                {
                    var r = x.attachedRigidbody;
                    Assert.IsNotNull(r, $"{x.name}に{typeof(Rigidbody)}がありません");
                    var vector = (actor.transform.position - r.transform.position) * leaveDistance;
                    actor.PostureController.Move(vector, true);
                })
                .AddTo(ct);
        }
    }
}
