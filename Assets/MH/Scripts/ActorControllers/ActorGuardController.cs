using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>のガード処理を制御するクラス
    /// </summary>
    public sealed class ActorGuardController : IActorController
    {
        private Actor actor;

        /// <summary>
        /// ガード中であるか
        /// </summary>
        public bool Guarding { private set; get; }

        /// <summary>
        /// ガードが可能か
        /// </summary>
        public bool CanGuard { private set; get; }

        private CancellationTokenSource beginTokenSource;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.actor = actor;
        }

        /// <summary>
        /// ガードを開始する
        /// </summary>
        public async UniTaskVoid Begin()
        {
            if (this.beginTokenSource != null)
            {
                return;
            }

            if (this.CanGuard)
            {
                this.BeginInternal();
            }
            else
            {
                try
                {
                    this.beginTokenSource = new CancellationTokenSource();
                    await UniTask.WaitUntil(() => !this.CanGuard, cancellationToken: this.beginTokenSource.Token);
                    this.BeginInternal();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// ガードを終了する
        /// </summary>
        public void End()
        {
            if (!this.Guarding)
            {
                return;
            }

            this.Guarding = false;
            this.DisposeToken();
            MessageBroker.GetPublisher<Actor, ActorEvents.EndGuard>()
                .Publish(this.actor, ActorEvents.EndGuard.Get());
        }

        /// <summary>
        /// ガードが出来る状態に設定する
        /// </summary>
        public void Validate()
        {
            this.CanGuard = true;
        }

        /// <summary>
        /// ガードが出来ない状態に設定する
        /// </summary>
        public void Invalidate()
        {
            this.End();
            this.CanGuard = false;
            this.DisposeToken();
        }

        private void BeginInternal()
        {
            this.Guarding = true;
            MessageBroker.GetPublisher<Actor, ActorEvents.BeginGuard>()
                .Publish(this.actor, ActorEvents.BeginGuard.Get());
        }

        private void DisposeToken()
        {
            if (this.beginTokenSource == null)
            {
                return;
            }

            this.beginTokenSource.Cancel();
            this.beginTokenSource.Dispose();
            this.beginTokenSource = null;
        }
    }
}
