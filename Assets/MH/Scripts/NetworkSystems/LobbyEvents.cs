using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.NetworkSystems
{
    /// <summary>
    /// ロビーに関するイベント
    /// </summary>
    public sealed class LobbyEvents
    {
        /// <summary>
        /// プレイヤーが追加された際のメッセージ
        /// </summary>
        public sealed class AddedPlayer : Message<AddedPlayer, Player>
        {
            public Player Player => this.Param1;
        }

        /// <summary>
        /// プレイヤーが削除された際のメッセージ
        /// </summary>
        public sealed class RemovedPlayer : Message<RemovedPlayer, Player>
        {
            public Player Player => this.Param1;
        }
    }
}
