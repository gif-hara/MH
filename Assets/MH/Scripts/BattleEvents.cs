using MessagePipe;

namespace MH
{
    /// <summary>
    /// バトルに関連するイベント
    /// </summary>
    public sealed class BattleEvents
    {
        /// <summary>
        /// 勝敗結果の判定をリクエストするメッセージ
        /// </summary>
        public sealed class RequestJudgeResult : Message<RequestJudgeResult, Define.BattleResult>
        {
            public Define.BattleResult Result => this.Param1;
        }

        /// <summary>
        /// 勝敗結果が決まった際のメッセージ
        /// </summary>
        public sealed class JudgedResult : Message<JudgedResult, Define.BattleResult>
        {
            public Define.BattleResult Result => this.Param1;
        }

        /// <summary>
        /// バトルの開始をリクエストするメッセージ
        /// </summary>
        public sealed class RequestBeginBattle : Message<RequestBeginBattle>
        {
        }

        /// <summary>
        /// イベントを登録する
        /// </summary>
        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<RequestJudgeResult>();
            builder.AddMessageBroker<JudgedResult>();
            builder.AddMessageBroker<RequestBeginBattle>();
        }
    }
}
