using Cysharp.Threading.Tasks;
using MH.UISystems;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MH
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public static class BootSystem
    {
        public static UniTask IsReady { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitializeOnBeforeSplashScreen()
        {
            IsReady = SetupInternal();
        }

        private static async UniTask SetupInternal()
        {
            await UniTask.WhenAll(
                SetupMessageBroker(),
                UIManager.SetupAsync(),
                SetupNetworkSystemAsync()
                );
            
            IsReady = UniTask.CompletedTask;
        }
        
        private static UniTask SetupMessageBroker()
        {
            MessageBroker.Setup(builder =>
            {
                ActorEvents.RegisterEvents(builder);
                TimeEvents.RegisterEvents(builder);
            });
            return UniTask.CompletedTask;
        }

        private static async UniTask SetupNetworkSystemAsync()
        {
            var prefab = await AssetLoader.LoadAsync<GameObject>("Assets/MH/Prefabs/NetworkSystems/NetworkManager.prefab");
            Object.Instantiate(prefab);
        }
    }
}
