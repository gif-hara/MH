using DG.Tweening;
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
        /// 回転のリクエストの受付を開始するメッセージ
        /// </summary>
        /// <remarks>
        /// 攻撃アニメーションのとあるタイミングで回転入力を受け付ける時に利用しています
        /// </remarks>
        public sealed class AcceptRequestRotation : Message<AcceptRequestRotation>
        {
        }

        /// <summary>
        /// 回転のリクエストの受付を終了するメッセージ
        /// </summary>
        /// <remarks>
        /// 攻撃アニメーションのとあるタイミングで回転入力を受け付ける時に利用しています
        /// </remarks>
        public sealed class CloseRequestRotation : Message<CloseRequestRotation>
        {
        }

        /// <summary>
        /// 回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestDodge : Message<RequestDodge, Vector3, float, float, Ease>
        {
            /// <summary>
            /// 回避の目標値
            /// </summary>
            public Vector3 Direction => this.Param1;

            /// <summary>
            /// 移動速度
            /// </summary>
            public float Speed => this.Param2;

            /// <summary>
            /// 回避する時間（秒）
            /// </summary>
            public float Duration => this.Param3;

            /// <summary>
            /// イージングタイプ
            /// </summary>
            public Ease Ease => this.Param4;
        }

        /// <summary>
        /// 攻撃のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestAttack : Message<RequestAttack>
        {
        }

        /// <summary>
        /// 攻撃が終了した際のメッセージ
        /// </summary>
        public sealed class EndAttack : Message<EndAttack>
        {
        }

        /// <summary>
        /// プレイヤーが生成された際のメッセージ
        /// </summary>
        public sealed class SpawnedPlayer : Message<SpawnedPlayer, Actor>
        {
            /// <summary>
            /// 生成されたプレイヤー
            /// </summary>
            public Actor Player => this.Param1;
        }

        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<Actor, BeginMove>();
            builder.AddMessageBroker<Actor, EndMove>();
            builder.AddMessageBroker<Actor, RequestMove>();
            builder.AddMessageBroker<Actor, RequestRotation>();
            builder.AddMessageBroker<Actor, AcceptRequestRotation>();
            builder.AddMessageBroker<Actor, CloseRequestRotation>();
            builder.AddMessageBroker<Actor, RequestDodge>();
            builder.AddMessageBroker<Actor, RequestAttack>();
            builder.AddMessageBroker<Actor, EndAttack>();
            builder.AddMessageBroker<SpawnedPlayer>();
        }
    }
}
