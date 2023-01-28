using System;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public sealed class ActorStatus
    {
        public Define.ActorType actorType;

        public int hitPoint;

        public ActorStatus(ActorStatus other)
        {
            this.actorType = other.actorType;
            this.hitPoint = other.hitPoint;
        }
    }
}
