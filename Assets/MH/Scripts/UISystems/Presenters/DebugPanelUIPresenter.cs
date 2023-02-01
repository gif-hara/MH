using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using MessagePipe;
using UnityEngine;

namespace MH.UISystems
{
    /// <summary>
    /// <see cref="DebugPanelUIView"/>を制御するクラス
    /// </summary>
    public sealed class DebugPanelUIPresenter
    {
        public DebugPanelUIPresenter(DebugPanelUIView uiViewPrefab, GameObject owner)
        {
            var uiView = UIManager.Open(uiViewPrefab);

            var ct = owner.GetCancellationTokenOnDestroy();
            owner.GetAsyncUpdateTrigger()
                .Subscribe(_ =>
                {
                    uiView.Text = "";
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<DebugPanelEvents.AppendLine>()
                .Subscribe(x =>
                {
                    uiView.Text += x.Message;
                })
                .AddTo(ct);
        }
    }
}
