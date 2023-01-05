using System;
using System.Net;
using Cysharp.Threading.Tasks;
using MessagePipe;
using MH.UISystems;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public static class BootSystem
    {
        public static UniTask IsReady { get; private set; }

        private static readonly DisposableBagBuilder bag = DisposableBag.CreateBuilder();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitializeOnBeforeSplashScreen()
        {
            IsReady = SetupInternal();
        }

        private static async UniTask SetupInternal()
        {
            await UniTask.WhenAll(
                SetupMessageBroker(),
                UIManager.SetupAsync()
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
    }
}
