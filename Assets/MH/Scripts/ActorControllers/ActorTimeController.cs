using System;
using Cysharp.Threading.Tasks;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の時間を制御するクラス
    /// </summary>
    public sealed class ActorTimeController
    {
        public ActorTimeController()
        {
            Time = new Time(TimeManager.Game);
        }
        public Time Time { get; }

        /// <summary>
        /// ヒットストップを開始する
        /// </summary>
        public async void BeginHitStop(float timeScale, float hitStopSeconds)
        {
            var tempTimeScale = Time.timeScale;
            Time.timeScale = timeScale;

            await UniTask.Delay(TimeSpan.FromSeconds(hitStopSeconds));

            Time.timeScale = tempTimeScale;
        }
    }
}
