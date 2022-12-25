using MessagePipe;
using UnityEngine;

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

        /// <summary>
        /// 移動のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestMove : Message<RequestMove, Vector3>
        {
            /// <summary>
            /// 移動量
            /// </summary>
            public Vector3 Velocity => this.Param1;
        }

        /// <summary>
        /// 回転のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestRotation : Message<RequestRotation, Quaternion>
        {
            /// <summary>
            /// 回転の目的地
            /// </summary>
            public Quaternion Rotation => this.Param1;
        }

        /// <summary>
        /// 回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestDodge : Message<RequestDodge, Vector3>
        {
            /// <summary>
            /// 回避の目標値
            /// </summary>
            public Vector3 Destination => this.Param1;
        }

        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<Actor, BeginMove>();
            builder.AddMessageBroker<Actor, EndMove>();
            builder.AddMessageBroker<Actor, RequestMove>();
            builder.AddMessageBroker<Actor, RequestRotation>();
            builder.AddMessageBroker<Actor, RequestDodge>();
        }
    }
}
