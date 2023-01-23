using UnityEngine;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="ActorSpawnData"/>を持つ<see cref="ScriptableObject"/>
    /// </summary>
    [CreateAssetMenu(menuName = "MH/ActorSpawnData")]
    public sealed class ActorSpawnDataScriptableObject : ScriptableObject
    {
        public ActorSpawnData data;
    }
}
