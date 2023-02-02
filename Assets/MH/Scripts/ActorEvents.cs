using MessagePipe;
using MH.ActorControllers;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>に関するイベント
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
            builder.AddMessageBroker<Actor, RequestDodge>();
            builder.AddMessageBroker<Actor, NetworkRequestDodge>();
            builder.AddMessageBroker<Actor, BeginDodge>();
            builder.AddMessageBroker<Actor, EndDodge>();
            builder.AddMessageBroker<Actor, RequestAttack>();
            builder.AddMessageBroker<Actor, BeginAttack>();
            builder.AddMessageBroker<Actor, EndAttack>();
            builder.AddMessageBroker<Actor, AcceptNextState>();
            builder.AddMessageBroker<AddedActor>();
            builder.AddMessageBroker<RemovedActor>();
            builder.AddMessageBroker<Actor, ValidationAttackCollider>();
            builder.AddMessageBroker<Actor, InvalidationAttackCollider>();
            builder.AddMessageBroker<Actor, RequestSetForce>();
            builder.AddMessageBroker<Actor, RequestUniqueMotion>();
            builder.AddMessageBroker<Actor, NetworkRequestUniqueMotion>();
            builder.AddMessageBroker<Actor, ChangedState>();
            builder.AddMessageBroker<Actor, GaveDamage>();
            builder.AddMessageBroker<Actor, ReceivedDamage>();
            builder.AddMessageBroker<Actor, Died>();
            builder.AddMessageBroker<Actor, RequestSubmitNewThinkData>();
            builder.AddMessageBroker<Actor, ReceivedNewThinkData>();
            builder.AddMessageBroker<Actor, BeginFlinch>();
            builder.AddMessageBroker<Actor, EndFlinch>();
        }

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
        /// 回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class RequestDodge : Message<RequestDodge, ActorDodgeController.InvokeData, bool>
        {
            /// <summary>
            /// 回避データ
            /// </summary>
            public ActorDodgeController.InvokeData Data => this.Param1;

            /// <summary>
            /// 強制的に実行するか
            /// </summary>
            public bool IsForce => this.Param2;
        }

        /// <summary>
        /// ネットワーク上で回避のリクエストを行うメッセージ
        /// </summary>
        public sealed class NetworkRequestDodge : Message<NetworkRequestDodge, ActorDodgeController.InvokeData>
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
        {
        }

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
        {
        }

        /// <summary>
        /// 次の行動を選択可能な状態にするメッセージ
        /// </summary>
        public sealed class AcceptNextState : Message<AcceptNextState>
        {
        }

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
        public sealed class ValidationAttackCollider : Message<ValidationAttackCollider, ValidationAttackColliderData>
        {
            /// <summary>
            /// データ
            /// </summary>
            public ValidationAttackColliderData Data => this.Param1;
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
        /// 衝撃値の設定をリクエストするメッセージ
        /// </summary>
        public sealed class RequestSetForce : Message<RequestSetForce, Vector3>
        {
            /// <summary>
            /// 衝撃値
            /// </summary>
            public Vector3 Force => this.Param1;
        }

        /// <summary>
        /// ユニークモーションの再生をリクエストするメッセージ
        /// </summary>
        public sealed class RequestUniqueMotion : Message<RequestUniqueMotion, string>
        {
            /// <summary>
            /// モーション名
            /// </summary>
            public string MotionName => this.Param1;
        }

        /// <summary>
        /// ネットワーク上でユニークモーションの再生をリクエストするメッセージ
        /// </summary>
        public sealed class NetworkRequestUniqueMotion : Message<NetworkRequestUniqueMotion, string>
        {
            /// <summary>
            /// モーション名
            /// </summary>
            public string MotionName => this.Param1;
        }

        /// <summary>
        /// ステートが切り替わった際のメッセージ
        /// </summary>
        public sealed class ChangedState : Message<ChangedState, ActorStateController.State, ActorStateController.State>
        {
            /// <summary>
            /// 前のステート
            /// </summary>
            public ActorStateController.State PreviousState => this.Param1;

            /// <summary>
            /// 今のステート
            /// </summary>
            public ActorStateController.State CurrentState => this.Param2;
        }

        /// <summary>
        /// ダメージを与えた際のメッセージ
        /// </summary>
        public sealed class GaveDamage : Message<GaveDamage, DamageData>
        {
            /// <summary>
            /// ダメージデータ
            /// </summary>
            public DamageData Data => this.Param1;
        }

        /// <summary>
        /// ダメージを受けた際のメッセージ
        /// </summary>
        public sealed class ReceivedDamage : Message<ReceivedDamage, DamageData>
        {
            /// <summary>
            /// ダメージデータ
            /// </summary>
            public DamageData Data => this.Param1;
        }

        /// <summary>
        /// 死亡した際のメッセージ
        /// </summary>
        public sealed class Died : Message<Died>
        {
        }

        /// <summary>
        /// 新しい思考データの送信のリクエストをするメッセージ
        /// </summary>
        /// <remarks>
        /// 現状敵のみ利用しています
        /// </remarks>
        public sealed class RequestSubmitNewThinkData : Message<RequestSubmitNewThinkData, Vector3, float, int>
        {
            /// <summary>
            /// 座標
            /// </summary>
            public Vector3 Position => this.Param1;

            /// <summary>
            /// 回転値
            /// </summary>
            public float RotationY => this.Param2;

            /// <summary>
            /// 乱数のシード値
            /// </summary>
            public int Seed => this.Param3;
        }

        /// <summary>
        /// サーバーから新しい姿勢を受け取った際のメッセージ
        /// </summary>
        /// <remarks>
        /// 現状敵のみ利用しています
        /// </remarks>
        public sealed class ReceivedNewThinkData : Message<ReceivedNewThinkData, Vector3, float, int>
        {
            /// <summary>
            /// 座標
            /// </summary>
            public Vector3 Position => this.Param1;

            /// <summary>
            /// Y回転軸
            /// </summary>
            public float RotationY => this.Param2;

            /// <summary>
            /// 乱数のシード値
            /// </summary>
            public int Seed => this.Param3;
        }

        /// <summary>
        /// ひるみを開始した際のメッセージ
        /// </summary>
        public sealed class BeginFlinch : Message<BeginFlinch>
        {
        }

        /// <summary>
        /// ひるみを終了した際のメッセージ
        /// </summary>
        public sealed class EndFlinch : Message<EndFlinch>
        {
        }

        /// <summary>
        /// ガードをリクエストするメッセージ
        /// </summary>
        public sealed class RequestGuard : Message<RequestGuard>
        {
        }
    }
}