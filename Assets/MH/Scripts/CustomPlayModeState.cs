using MH.NetworkSystems;
using UnityEditor;

namespace MH
{
#if UNITY_EDITOR
    /// <summary>
    /// Unityのプレイモードを検知するクラス
    /// </summary>
    [InitializeOnLoad]
    public static class CustomPlayModeState
    {
        static CustomPlayModeState()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            // Enter Play Mode SettingsのDomainやSceneのリロードをオフにしているため
            // staticなものはここで初期化を行うようにしています
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                LobbyManager.OnEnteredPlayMode();
            }
        }
    }
#endif
}
