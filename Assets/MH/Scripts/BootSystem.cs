using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace Cookie
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public static class BootSystem
    {
        public static UniTask IsReady { get; private set; }

        private static readonly DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Setup()
        {
            IsReady = SetupInternal();
        }

        private static async UniTask SetupInternal()
        {
            await UniTask.WhenAll(
                SetupMessageBroker()
                );
            
            IsReady = UniTask.CompletedTask;
        }
        
        private static UniTask SetupMessageBroker()
        {
            MessageBroker.Setup(builder =>
            {
            });
            return UniTask.CompletedTask;
        }
    }
}
