using UnityEngine;

namespace MH.SceneControllers
{
    /// <summary>
    /// バトルシーン用のデバッグデータ
    /// </summary>
    [CreateAssetMenu(menuName = "MH/BattleSceneDebugData")]
    public sealed class BattleSceneDebugData : ScriptableObject
    {
        public ActorSpawnDataScriptableObject playerSpawnData;

        public ActorSpawnDataScriptableObject enemySpawnData;
    }
}
