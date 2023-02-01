using MessagePipe;

namespace MH
{
    /// <summary>
    /// デバッグパネル関連のイベント
    /// </summary>
    public sealed class DebugPanelEvents
    {
        /// <summary>
        /// イベントの登録を行う
        /// </summary>
        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<AppendLine>();
        }

        /// <summary>
        /// パネルに一行メッセージを追加する
        /// </summary>
        public sealed class AppendLine : Message<AppendLine, string>
        {
            /// <summary>
            /// 追加したいメッセージ
            /// </summary>
            public string Message => this.Param1;
        }
    }
}
