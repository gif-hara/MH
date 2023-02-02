using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>のガード処理を制御するクラス
    /// </summary>
    public sealed class ActorGuardController : IActorController
    {
        /// <summary>
        /// ガード中であるか
        /// </summary>
        public bool Guarding { private set; get; }

        /// <summary>
        /// ガードが可能か
        /// </summary>
        public bool CanGuard { set; get; }

        private CancellationTokenSource beginTokenSource;

        public void Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
            )
        {
        }

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
                    await UniTask.WaitUntil(() => !this.CanGuard, cancellationToken:this.beginTokenSource.Token);
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

        public void End()
        {
            this.Guarding = false;
            this.beginTokenSource?.Cancel();
            this.beginTokenSource?.Dispose();
            this.beginTokenSource = null;
        }

        private void BeginInternal()
        {
            this.Guarding = true;
        }
    }
}