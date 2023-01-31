using System.Collections.Generic;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>を管理するクラス
    /// </summary>
    public static class ActorManager
    {
        private static readonly List<Actor> actors = new();

        private static readonly List<Actor> players = new();

        private static readonly List<Actor> enemies = new();

        private static readonly Dictionary<ulong, Actor> networkObjectIdMap = new();

        public static IReadOnlyList<Actor> Actors => actors;

        public static IReadOnlyList<Actor> Players => players;

        public static IReadOnlyList<Actor> Enemies => enemies;

        public static Actor OwnerActor
        {
            get
            {
                foreach (var player in Players)
                {
                    if (player.NetworkController.IsOwner)
                    {
                        return player;
                    }
                }

                Assert.IsTrue(false, "OwnerなActorが存在しません");
                return null;
            }
        }

        public static void AddActor(Actor actor)
        {
            actors.Add(actor);
            networkObjectIdMap.Add(actor.NetworkController.NetworkObjectId, actor);

            if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
            {
                players.Add(actor);
            }
            else if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Enemy)
            {
                enemies.Add(actor);
            }

            MessageBroker.GetPublisher<ActorEvents.AddedActor>()
                .Publish(ActorEvents.AddedActor.Get(actor));
        }

        public static void RemoveActor(Actor actor)
        {
            actors.Remove(actor);
            networkObjectIdMap.Remove(actor.NetworkController.NetworkObjectId);

            if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
            {
                players.Remove(actor);
            }
            else if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Enemy)
            {
                enemies.Remove(actor);
            }

            MessageBroker.GetPublisher<ActorEvents.RemovedActor>()
                .Publish(ActorEvents.RemovedActor.Get(actor));
        }

        /// <summary>
        /// NetworkObjectIdから<see cref="Actor"/>を返す
        /// </summary>
        public static Actor GetActorFromNetworkObjectId(ulong networkObjectId)
        {
            Assert.IsTrue(networkObjectIdMap.ContainsKey(networkObjectId), $"{nameof(networkObjectId)} = {networkObjectId} is not found.");
            return networkObjectIdMap[networkObjectId];
        }
    }
}
