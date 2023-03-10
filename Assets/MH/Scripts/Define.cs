namespace MH
{
    /// <summary>
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

        /// <summary>
        /// 部位タイプ
        /// </summary>
        public enum PartType
        {
            Body,
        }

        /// <summary>
        /// バトル勝敗結果
        /// </summary>
        public enum BattleResult
        {
            PlayerWin,
            PlayerLose,
        }

        public const int SpecialGaugeMax = 100;

        public const int SpecialTankMax = 3;
    }
}
