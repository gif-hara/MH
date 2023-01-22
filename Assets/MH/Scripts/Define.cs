namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public static class Define
    {

        /// <summary>
        /// アクタータイプ
        /// </summary>
        public enum ActorType
        {
            Player,

            Enemy,
        }

        /// <summary>
        /// アタックリクエストタイプ
        /// </summary>
        public enum RequestAttackType
        {
            /// <summary>
            /// 弱攻撃
            /// </summary>
            Weak,

            /// <summary>
            /// 強攻撃
            /// </summary>
            Strong,
        }
    }
}
