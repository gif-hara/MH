using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// <see cref="Time"/>に関するイベント
    /// </summary>
    public sealed class PoolablePrefabEvents
    {
        /// <summary>
        /// プレハブの生成をリクエストするメッセージ
        /// </summary>
        public sealed class RequestCreate : Message<RequestCreate, PoolablePrefab, Vector3, Quaternion>
        {
            public PoolablePrefab Prefab => this.Param1;

            public Vector3 Position => this.Param2;

            public Quaternion Rotation => this.Param3;
        }

        /// <summary>
        /// イベントの登録を行う
        /// </summary>
        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<RequestCreate>();
        }
    }
}
