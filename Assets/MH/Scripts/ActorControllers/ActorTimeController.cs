using System;
using Cysharp.Threading.Tasks;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の時間を制御するクラス
    /// </summary>
    public sealed class ActorTimeController
    {
        public Time Time { get; }

        public ActorTimeController()
        {
            this.Time = new Time(TimeManager.Game);
        }

        /// <summary>
        /// ヒットストップを開始する
        /// </summary>
        public async void BeginHitStop(float timeScale, float hitStopSeconds)
        {
            var tempTimeScale = this.Time.timeScale;
            this.Time.timeScale = timeScale;

            await UniTask.Delay(TimeSpan.FromSeconds(hitStopSeconds));

            this.Time.timeScale = tempTimeScale;
        }
    }
}
