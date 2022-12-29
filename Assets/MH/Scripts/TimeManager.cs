namespace MH
{
    /// <summary>
    /// <see cref="Time"/>を管理するクラス
    /// </summary>
    public static class TimeManager
    {
        public static readonly Time System = new ();

        public static readonly Time Game = new (System);

        public static Time Create(Time parent = null)
        {
            return new Time(parent);
        }
    }
}
