using Cysharp.Threading.Tasks;
using MH.ActorControllers;
using MH.UISystems;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public static class BootSystem
    {
        public static UniTask IsReady { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeOnBeforeSplashScreen()
        {
            IsReady = SetupInternal();
        }

        private static async UniTask SetupInternal()
        {
            await UniTask.WhenAll(
                SetupMessageBrokerAsync(),
                UIManager.SetupAsync(),
                SetupNetworkSystemAsync(),
                SetupInputActionAsync(),
                PlayerActorCommonData.SetupAsync(),
                UniTask.DelayFrame(1)
                );

            IsReady = UniTask.CompletedTask;
        }

        private static UniTask SetupMessageBrokerAsync()
        {
            MessageBroker.Setup(builder =>
            {
                ActorEvents.RegisterEvents(builder);
                TimeEvents.RegisterEvents(builder);
                PoolablePrefabEvents.RegisterEvents(builder);
                DebugPanelEvents.RegisterEvents(builder);
                BattleEvents.RegisterEvents(builder);
            });
            return UniTask.CompletedTask;
        }

        private static async UniTask SetupNetworkSystemAsync()
        {
            var prefab = await AssetLoader.LoadAsync<GameObject>("Assets/MH/Prefabs/NetworkSystems/NetworkManager.prefab");
            Object.Instantiate(prefab);
        }

        private static UniTask SetupInputActionAsync()
        {
            InputController.Setup();
            return UniTask.CompletedTask;
        }
    }
}
