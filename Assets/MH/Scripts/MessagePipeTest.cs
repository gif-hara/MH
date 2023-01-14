using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.InputSystem;

public class MessagePipeTest : MonoBehaviour
{
    private CancellationTokenSource cancellationTokenSource;

    async void Start()
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe();
        builder.AddMessageBroker<MyEvent>();
        var provider = builder.BuildServiceProvider();
        GlobalMessagePipe.SetProvider(provider);

        var publisher = GlobalMessagePipe.GetPublisher<MyEvent>();
        var subscriber = GlobalMessagePipe.GetSubscriber<MyEvent>();
        var asyncPublisher = GlobalMessagePipe.GetAsyncPublisher<MyEvent>();
        var asyncSubscriber = GlobalMessagePipe.GetAsyncSubscriber<MyEvent>();

        var d = subscriber
            .Subscribe(x => Debug.Log(x.message));
        var e = asyncSubscriber
            .Subscribe(async (myEvent, cancelToken) =>
            {
                await UniTask.Delay(3000, cancellationToken: cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelされた");
                }
                else
                {
                    Debug.Log(myEvent.message);
                }
            });
        var f = asyncSubscriber
            .Subscribe(async (myEvent, cancelToken) =>
            {
                await UniTask.Delay(1000, cancellationToken: cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    Debug.Log("Cancelされた");
                }
                else
                {
                    Debug.Log(myEvent.message);
                }
            });

        publisher.Publish(new MyEvent{message = "Test"});
        publisher.Publish(new MyEvent{message = "Hoge"});
        publisher.Publish(new MyEvent{message = "Fuga"});

        this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        try
        {
            await asyncPublisher.PublishAsync(new MyEvent {message = "Async"}, this.cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        Debug.Log("Complete!");

        d.Dispose();
        e.Dispose();
        f.Dispose();
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            this.cancellationTokenSource.Cancel();
            Debug.Log("Cancel");
        }
    }
}

public class MyEvent
{
    public string message;
}
