using MH.ActorControllers;

namespace MH
{
    /// <summary>
    /// ダメージを与えた際のデータ
    /// </summary>
    public struct DamageData
    {
        public int damage;

        /// <summary>
        /// ダメージを受けた<see cref="Actor"/>
        /// </summary>
        public Actor receiveActor;
    }
}
