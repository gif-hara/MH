using UnityEngine;
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

        private void Start()
        {
            SceneManager.LoadScene(this.sceneName);
        }
    }
}
