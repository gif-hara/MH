using MessagePipe;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Time"/>に関するイベント
    /// </summary>
    public sealed class TimeEvents
    {
        /// <summary>
        /// タイムスケールに更新があった際のメッセージ
        /// </summary>
        public sealed class UpdatedTimeScale : Message<UpdatedTimeScale>
        {
        }
        
        /// <summary>
        /// イベントの登録を行う
        /// </summary>
        public static void RegisterEvents(BuiltinContainerBuilder builder)
        {
            builder.AddMessageBroker<Time, UpdatedTimeScale>();
        }
    }
}
