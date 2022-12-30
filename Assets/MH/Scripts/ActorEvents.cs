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
        /// 次の行動を選択可能な状態にするメッセージ
        /// </summary>
        public sealed class AcceptNextState : Message<AcceptNextState>
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

        /// <summary>
        /// 攻撃判定の有効化を行うメッセージ
        /// </summary>
        public sealed class ValidationAttackCollider : Message<ValidationAttackCollider, string>
        {
            /// <summary>
            /// 有効にする攻撃判定の名前
            /// </summary>
            public string ColliderName => this.Param1;
        }

        /// <summary>
        /// 攻撃判定の無効化を行うメッセージ
        /// </summary>
        public sealed class InvalidationAttackCollider : Message<InvalidationAttackCollider, string>
        {
            /// <summary>
            /// 無効にする攻撃判定の名前
            /// </summary>
            public string ColliderName => this.Param1;
        }

        /// <summary>
        /// 攻撃が当たった際のメッセージ
        /// </summary>
        public sealed class HitAttack : Message<HitAttack, HitData>
        {
            /// <summary>
            /// ヒットデータ
            /// </summary>
            public HitData HitData => this.Param1;
        }

        /// <summary>
        /// イベントの登録を行う
        /// </summary>
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
            builder.AddMessageBroker<Actor, AcceptNextState>();
            builder.AddMessageBroker<SpawnedPlayer>();
            builder.AddMessageBroker<Actor, ValidationAttackCollider>();
            builder.AddMessageBroker<Actor, InvalidationAttackCollider>();
            builder.AddMessageBroker<HitAttack>();
        }
    }
}
