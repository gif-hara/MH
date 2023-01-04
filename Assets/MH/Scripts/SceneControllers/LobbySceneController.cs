using System;
using MH.UISystems;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbySceneController : MonoBehaviour
    {
        [SerializeField]
        private LobbyUIView lobbyUIViewPrefab;
        
        private async void Start()
        {
            await BootSystem.IsReady;

            var uiView = UIManager.Open(this.lobbyUIViewPrefab);
        }
    }
}
