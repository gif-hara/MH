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
        private const float actorRadiusOffset = 0.2f;

        public void Setup(Actor actor, IActorDependencyInjector actorDependencyInjector, ActorSpawnData spawnData)
        {
            var ct = actor.GetCancellationTokenOnDestroy();
            actor.GetAsyncTriggerStayTrigger()
                .Subscribe(x =>
                {
                    var r = x.attachedRigidbody;
                    Assert.IsNotNull(r, $"{x.name}に{typeof(Rigidbody)}がありません");

                    if (x is BoxCollider boxCollider)
                    {
                        var t = boxCollider.transform;
                        var forward = t.forward;
                        var right = t.right;
                        var size = boxCollider.size;
                        var originPosition = t.position + boxCollider.center;
                        var actorPosition = actor.transform.position;
                        var actorRadius = actor.PostureController.Radius + actorRadiusOffset;
                        var actorDot = Vector3.Dot(
                            (actorPosition - originPosition).normalized,
                            forward
                            );
                        var rightDot = Dot(size.x, size.z, t, forward);

                        // 前方にめり込んでいる場合
                        if (actorDot > rightDot && actorDot <= 1.0f)
                        {
                            var projection = Vector3.Project(actor.transform.position - originPosition, right);
                            var totalLength = originPosition + projection + forward * (size.z + actorRadius);
                            var leaveVector = totalLength - actorPosition;
                            leaveVector.y = 0.0f;
                            actor.PostureController.Move(leaveVector, true);
                            return;
                        }

                        var leftDot = rightDot;
                        rightDot = Dot(size.x, -size.z, t, forward);

                        // 横方向にめり込んでいる場合
                        if (actorDot <= leftDot && actorDot > rightDot)
                        {
                            var actorHorizontalDot = Vector3.Dot(
                                (actorPosition - originPosition).normalized,
                                right
                                );
                            var projection = Vector3.Project(actor.transform.position - originPosition, forward);
                            var totalLength = originPosition + projection;

                            // 左右どちらにめり込んでいるかで移動量の計算を分ける
                            totalLength += actorHorizontalDot < 0.0f
                                ? -right * (size.x + actorRadius)
                                : right * (size.x + actorRadius);
                            var leaveVector = totalLength - actorPosition;
                            leaveVector.y = 0.0f;

                            actor.PostureController.Move(leaveVector, true);
                            return;
                        }

                        leftDot = rightDot;

                        // 後方にめり込んでいる場合
                        if (actorDot < leftDot && actorDot >= -1.0f)
                        {
                            var projection = Vector3.Project(actor.transform.position - originPosition, right);
                            var totalLength = originPosition + projection + -forward * (size.z + actorRadius);
                            var leaveVector = totalLength - actorPosition;
                            leaveVector.y = 0.0f;
                            actor.PostureController.Move(leaveVector, true);
                        }
                    }
                })
                .AddTo(ct);
        }

        private static float Dot(float x, float z, Transform t, Vector3 rhs)
        {
            var right = t.right * x;
            right.y = 0.0f;
            var forward = t.forward * z;
            forward.y = 0.0f;
            var lhs = (right + forward).normalized;
            return Vector3.Dot(
                lhs,
                rhs
                );
        }
    }
}
