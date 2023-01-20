using System;
using Cysharp.Threading.Tasks;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の時間を制御するクラス
    /// </summary>
    public sealed class ActorTimeController : IActorController
    {
        public Time Time { get; private set; }

        void IActorController.Setup(
            Actor actor,
            IActorDependencyInjector actorDependencyInjector,
            ActorSpawnData spawnData
        )
        {
            this.Time = new Time(TimeManager.Game);
        }

        /// <summary>
        /// ヒットストップを開始する
        /// </summary>
        public async void BeginHitStop(float timeScale, float hitStopSeconds)
        {
            this.Time.timeScale = timeScale;

            await UniTask.Delay(TimeSpan.FromSeconds(hitStopSeconds));

            this.Time.timeScale = 1.0f;
        }
    }
}
