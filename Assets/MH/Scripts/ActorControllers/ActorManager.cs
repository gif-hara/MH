using System.Collections.Generic;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public static class ActorManager
    {
        private static readonly List<Actor> actors = new();

        private static readonly List<Actor> players = new();

        private static readonly List<Actor> enemies = new();

        public static IReadOnlyList<Actor> Actors => actors;

        public static IReadOnlyList<Actor> Players => players;

        public static IReadOnlyList<Actor> Enemies => enemies;

        public static void AddActor(Actor actor)
        {
            actors.Add(actor);

            if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
            {
                players.Add(actor);
            }
            else if(actor.StatusController.BaseStatus.actorType == Define.ActorType.Enemy)
            {
                enemies.Add(actor);
            }

            MessageBroker.GetPublisher<ActorEvents.AddedActor>()
                .Publish(ActorEvents.AddedActor.Get(actor));
        }

        public static void RemoveActor(Actor actor)
        {
            actors.Remove(actor);

            if (actor.StatusController.BaseStatus.actorType == Define.ActorType.Player)
            {
                players.Remove(actor);
            }
            else if(actor.StatusController.BaseStatus.actorType == Define.ActorType.Enemy)
            {
                enemies.Remove(actor);
            }

            MessageBroker.GetPublisher<ActorEvents.RemovedActor>()
                .Publish(ActorEvents.RemovedActor.Get(actor));
        }
    }
}
