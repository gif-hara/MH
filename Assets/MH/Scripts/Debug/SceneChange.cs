using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace MH.DebugSystems
{
    /// <summary>
    /// シーンを切り替えるだけのデバッグクラス
    /// </summary>
    public sealed class SceneChange : MonoBehaviour
    {
        [SerializeField]
        private string sceneName;
        
        private async void Start()
        {
            SceneManager.LoadScene(this.sceneName);
        }
    }
}
