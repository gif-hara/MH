using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using MH.NetworkSystems;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorAIGhostPlayer : IActorAI
    {
        private readonly PlayerNetworkBehaviour playerNetworkBehaviour;

        private Actor actor;

        public ActorAIGhostPlayer(PlayerNetworkBehaviour playerNetworkBehaviour)
        {
            this.playerNetworkBehaviour = playerNetworkBehaviour;
            Assert.IsFalse(this.playerNetworkBehaviour.IsOwner);
        }

        public void Attach(Actor actor)
        {
            this.actor = actor;
            var ct = this.actor.GetCancellationTokenOnDestroy();
            this.actor.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    var playerActorCommonData = PlayerActorCommonData.Instance;

                    // 座標の更新
                    {
                        var networkPosition = this.playerNetworkBehaviour.NetworkPosition;
                        var difference = networkPosition - this.actor.transform.localPosition;
                        var threshold = playerActorCommonData.WarpPositionThreshold;
                        if (difference.sqrMagnitude > threshold * threshold)
                        {
                            this.actor.PostureController.Warp(networkPosition);
                        }
                        else
                        {
                            var sqrMagnitude = difference.sqrMagnitude;
                            threshold = playerActorCommonData.MoveSpeed * playerActorCommonData.MoveSpeed;
                            if (sqrMagnitude >= threshold)
                            {
                                var direction = difference.normalized;
                                this.actor.PostureController.Move(direction * playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime);
                            }
                            else if (sqrMagnitude < threshold && sqrMagnitude > 0.01f)
                            {
                                this.actor.PostureController.Move(difference * playerActorCommonData.MoveSpeed * this.actor.TimeController.Time.deltaTime);
                            }
                        }
                    }

                    // 回転の更新
                    {
                        var rotation = Quaternion.Lerp(
                            this.actor.transform.localRotation,
                            Quaternion.Euler(0.0f, this.playerNetworkBehaviour.NetworkRotation, 0.0f),
                            playerActorCommonData.RotationSpeed * this.actor.TimeController.Time.deltaTime
                            );
                        this.actor.PostureController.Rotate(rotation);
                    }
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.NetworkRequestDodge>()
                .Subscribe(this.actor, x =>
                {
                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestDodge>()
                        .Publish(this.actor, ActorEvents.RequestDodge.Get(x.Data, true));
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.NetworkRequestUniqueMotion>()
                .Subscribe(this.actor, x =>
                {
                    MessageBroker.GetPublisher<Actor, ActorEvents.RequestUniqueMotion>()
                        .Publish(this.actor, ActorEvents.RequestUniqueMotion.Get(x.MotionName));
                })
                .AddTo(ct);
        }
    }
}
