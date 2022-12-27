using System;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>を生成するために必要なデータ
    /// </summary>
    [Serializable]
    public sealed class ActorSpawnData
    {
        public ActorAttackData attackData;
    }
}
