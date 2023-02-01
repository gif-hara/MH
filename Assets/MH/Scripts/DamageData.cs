using MH.ActorControllers;

namespace MH
{
    /// <summary>
    /// ダメージを与えた際のデータ
    /// </summary>
    public struct DamageData
    {
        /// <summary>
        /// ダメージ
        /// </summary>
        public int damage;

        /// <summary>
        /// ダメージを受けた<see cref="Actor"/>
        /// </summary>
        public Actor receiveActor;

        /// <summary>
        /// ダメージを受けた部位
        /// </summary>
        public Define.PartType partType;
    }
}
