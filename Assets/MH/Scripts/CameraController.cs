using Cinemachine;
using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField]
        private CinemachineVirtualCamera player;

        public static CameraController Instance { private set; get; }

        public CinemachineVirtualCamera Player => this.player;

        private void Awake()
        {
            Instance = this;
        }
    }
}
