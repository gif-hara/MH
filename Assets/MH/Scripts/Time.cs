namespace MH
{
    /// <summary>
    /// 時間に関するクラス
    /// </summary>
    public sealed class Time
    {
        private readonly Time parent;

        private float _timeScale = 1.0f;

        public float timeScale
        {
            set
            {
                this._timeScale = value;
                MessageBroker.GetPublisher<Time, TimeEvents.UpdatedTimeScale>()
                    .Publish(this, TimeEvents.UpdatedTimeScale.Get(value));
            }
            get => this._timeScale;
        }

        public float totalTimeScale => this.GetTimeScaleRecursive(1.0f);

        private float GetTimeScaleRecursive(float value)
        {
            if (this.parent != null)
            {
                return this.parent.GetTimeScaleRecursive(value * this.timeScale);
            }
            else
            {
                return value * this.timeScale;
            }
        }

        public float deltaTime => UnityEngine.Time.deltaTime * this.totalTimeScale;

        public Time(Time parent = null)
        {
            this.parent = parent;
        }
    }
}
