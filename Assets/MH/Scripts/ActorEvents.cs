using MessagePipe;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActorEvents
    {
        /// <summary>
        /// 移動を開始した際のメッセージ
        /// </summary>
        public sealed class BeginMove : Message<BeginMove>
        {
        }
        
        /// <summary>
        /// 移動を終了した際のメッセージ
        /// </summary>
        public sealed class EndMove : Message<EndMove>
        {
        }

        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<Actor, BeginMove>();
            builder.AddMessageBroker<Actor, EndMove>();
        }
    }
}
