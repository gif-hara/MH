using MessagePipe;
using MH.ActorControllers;
using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ActorEvents
    {

        /// <summary>
        /// イベントの登録を行う
        /// </summary>
        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<Actor, BeginMove>();
            builder.AddMessageBroker<Actor, EndMove>();
            builder.AddMessageBroker<Actor, RequestMove>();
            builder.AddMessageBroker<Actor, RequestDodge>();
            builder.AddMessageBroker<Actor, RequestDodgeNetwork>();
            builder.AddMessageBroker<Actor, BeginDodge>();
            builder.AddMessageBroker<Actor, EndDodge>();
            builder.AddMessageBroker<Actor, RequestAttack>();
            builder.AddMessageBroker<Actor, RequestAttackNetwork>();
            builder.AddMessageBroker<Actor, BeginAttack>();
            builder.AddMessageBroker<Actor, EndAttack>();
            builder.AddMessageBroker<Actor, AcceptNextState>();
            builder.AddMessageBroker<AddedActor>();
            builder.AddMessageBroker<RemovedActor>();
            builder.AddMessageBroker<Actor, ValidationAttackCollider>();
            builder.AddMessageBroker<Actor, InvalidationAttackCollider>();
            builder.AddMessageBroker<HitAttack>();
            builder.AddMessageBroker<Actor, RequestSetForce>();
        }

        /// <summary>
        /// 移動を開始した際のメッセージ
        /// </summary>
        public sealed class BeginMove : Message<BeginMove>
        {}

        /// <summary>
        /// 移動を終了した際のメッセージ
        /// </summary>
        public sealed class EndMove : Message<EndMove>
        {}

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
        /// 回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestDodge : Message<RequestDodge, ActorDodgeController.InvokeData>
        {
            public ActorDodgeController.InvokeData Data => this.Param1;
        }

        /// <summary>
        /// ネットワーク上で回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestDodgeNetwork : Message<RequestDodgeNetwork, ActorDodgeController.InvokeData>
        {
            public ActorDodgeController.InvokeData Data => this.Param1;
        }

        /// <summary>
        /// 回避を開始した際のメッセージ
        /// </summary>
        public sealed class BeginDodge : Message<BeginDodge, ActorDodgeController.InvokeData>
        {
            public ActorDodgeController.InvokeData Data => this.Param1;
        }

        /// <summary>
        /// 回避が終了した際のメッセージ
        /// </summary>
        public sealed class EndDodge : Message<EndDodge>
        {}

        /// <summary>
        /// 攻撃のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestAttack : Message<RequestAttack, Define.RequestAttackType>
        {
            /// <summary>
            /// リクエストタイプ
            /// </summary>
            public Define.RequestAttackType AttackType => this.Param1;
        }

        /// <summary>
        /// ネットワーク上で攻撃のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestAttackNetwork : Message<RequestAttackNetwork, string>
        {
            /// <summary>
            /// モーションの名前
            /// </summary>
            public string MotionName => this.Param1;
        }

        /// <summary>
        /// 攻撃が開始した際のメッセージ
        /// </summary>
        public sealed class BeginAttack : Message<BeginAttack, string>
        {
            /// <summary>
            /// モーションの名前
            /// </summary>
            public string MotionName => this.Param1;
        }

        /// <summary>
        /// 攻撃が終了した際のメッセージ
        /// </summary>
        public sealed class EndAttack : Message<EndAttack>
        {}

        /// <summary>
        /// 次の行動を選択可能な状態にするメッセージ
        /// </summary>
        public sealed class AcceptNextState : Message<AcceptNextState>
        {}

        /// <summary>
        /// <see cref="Actor"/>が追加された際のメッセージ
        /// </summary>
        public sealed class AddedActor : Message<AddedActor, Actor>
        {
            /// <summary>
            /// 追加された<see cref="Actor"/>
            /// </summary>
            public Actor Actor => this.Param1;
        }

        /// <summary>
        /// <see cref="Actor"/>が削除された際のメッセージ
        /// </summary>
        public sealed class RemovedActor : Message<RemovedActor, Actor>
        {
            /// <summary>
            /// 削除された<see cref="Actor"/>
            /// </summary>
            public Actor Actor => this.Param1;
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
        /// 衝撃値の設定をリクエストするメッセージ
        /// </summary>
        public sealed class RequestSetForce : Message<RequestSetForce, Vector3>
        {
            /// <summary>
            /// 衝撃値
            /// </summary>
            public Vector3 Force => this.Param1;
        }
    }
}
