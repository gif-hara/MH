using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MH
{
    public class ActionHandlerAsyncEnumerable : IUniTaskAsyncEnumerable<AsyncUnit>
    {
        readonly CancellationToken cancellationToken1;
        private readonly Action<Action> disposeAction;
        private readonly Action<Action> registerAction;

        public ActionHandlerAsyncEnumerable(Action<Action> registerAction, Action<Action> disposeAction, CancellationToken cancellationToken)
        {
            this.cancellationToken1 = cancellationToken;
            this.registerAction = registerAction;
            this.disposeAction = disposeAction;
        }

        public IUniTaskAsyncEnumerator<AsyncUnit> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this.cancellationToken1 == cancellationToken)
            {
                return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, CancellationToken.None);
            }
            return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, cancellationToken);
        }

        class ActionHandlerAsyncEnumerator : MoveNextSource, IUniTaskAsyncEnumerator<AsyncUnit>
        {
            static readonly Action<object> cancel1 = OnCanceled1;
            static readonly Action<object> cancel2 = OnCanceled2;

            readonly Action action;
            readonly CancellationToken cancellationToken1;
            readonly CancellationToken cancellationToken2;
            private readonly Action<Action> disposeAction;
            private readonly Action<Action> registerAction;
            bool isDisposed;

            bool isInitialized;
            CancellationTokenRegistration registration1;
            CancellationTokenRegistration registration2;

            public ActionHandlerAsyncEnumerator(Action<Action> registerAction, Action<Action> disposeAction, CancellationToken cancellationToken1, CancellationToken cancellationToken2)
            {
                this.action = Invoke;
                this.registerAction = registerAction;
                this.disposeAction = disposeAction;
                this.cancellationToken1 = cancellationToken1;
                this.cancellationToken2 = cancellationToken2;
            }

            public AsyncUnit Current => default;

            public UniTask<bool> MoveNextAsync()
            {
                cancellationToken1.ThrowIfCancellationRequested();
                cancellationToken2.ThrowIfCancellationRequested();
                completionSource.Reset();

                if (!isInitialized)
                {
                    isInitialized = true;

                    TaskTracker.TrackActiveTask(this, 3);
                    this.registerAction(action);
                    if (cancellationToken1.CanBeCanceled)
                    {
                        registration1 = cancellationToken1.RegisterWithoutCaptureExecutionContext(cancel1, this);
                    }
                    if (cancellationToken2.CanBeCanceled)
                    {
                        registration2 = cancellationToken2.RegisterWithoutCaptureExecutionContext(cancel2, this);
                    }
                }

                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    TaskTracker.RemoveTracking(this);
                    registration1.Dispose();
                    registration2.Dispose();
                    this.disposeAction(action);

                    completionSource.TrySetCanceled();
                }

                return default;
            }

            void Invoke()
            {
                completionSource.TrySetResult(true);
            }

            static void OnCanceled1(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken1);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }

            static void OnCanceled2(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken2);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }
        }
    }

    public class ActionHandlerAsyncEnumerable<T> : IUniTaskAsyncEnumerable<T>
    {
        readonly CancellationToken cancellationToken1;
        private readonly Action<Action<T>> disposeAction;
        private readonly Action<Action<T>> registerAction;

        public ActionHandlerAsyncEnumerable(Action<Action<T>> registerAction, Action<Action<T>> disposeAction, CancellationToken cancellationToken)
        {
            this.cancellationToken1 = cancellationToken;
            this.registerAction = registerAction;
            this.disposeAction = disposeAction;
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this.cancellationToken1 == cancellationToken)
            {
                return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, CancellationToken.None);
            }
            return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, cancellationToken);
        }

        class ActionHandlerAsyncEnumerator : MoveNextSource, IUniTaskAsyncEnumerator<T>
        {
            static readonly Action<object> cancel1 = OnCanceled1;
            static readonly Action<object> cancel2 = OnCanceled2;

            readonly Action<T> action;
            readonly CancellationToken cancellationToken1;
            readonly CancellationToken cancellationToken2;
            private readonly Action<Action<T>> disposeAction;
            private readonly Action<Action<T>> registerAction;
            bool isDisposed;

            bool isInitialized;
            CancellationTokenRegistration registration1;
            CancellationTokenRegistration registration2;

            public ActionHandlerAsyncEnumerator(Action<Action<T>> registerAction, Action<Action<T>> disposeAction, CancellationToken cancellationToken1, CancellationToken cancellationToken2)
            {
                this.action = Invoke;
                this.registerAction = registerAction;
                this.disposeAction = disposeAction;
                this.cancellationToken1 = cancellationToken1;
                this.cancellationToken2 = cancellationToken2;
            }

            public T Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                cancellationToken1.ThrowIfCancellationRequested();
                cancellationToken2.ThrowIfCancellationRequested();
                completionSource.Reset();

                if (!isInitialized)
                {
                    isInitialized = true;

                    TaskTracker.TrackActiveTask(this, 3);
                    this.registerAction(action);
                    if (cancellationToken1.CanBeCanceled)
                    {
                        registration1 = cancellationToken1.RegisterWithoutCaptureExecutionContext(cancel1, this);
                    }
                    if (cancellationToken2.CanBeCanceled)
                    {
                        registration2 = cancellationToken2.RegisterWithoutCaptureExecutionContext(cancel2, this);
                    }
                }

                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    TaskTracker.RemoveTracking(this);
                    registration1.Dispose();
                    registration2.Dispose();
                    if (action is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                    this.disposeAction(action);

                    completionSource.TrySetCanceled();
                }

                return default;
            }

            void Invoke(T value)
            {
                Current = value;
                completionSource.TrySetResult(true);
            }

            static void OnCanceled1(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken1);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }

            static void OnCanceled2(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken2);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }
        }
    }

    public class ActionHandlerAsyncEnumerable<T1, T2> : IUniTaskAsyncEnumerable<Tuple<T1, T2>>
    {
        readonly CancellationToken cancellationToken1;
        private readonly Action<Action<T1, T2>> disposeAction;
        private readonly Action<Action<T1, T2>> registerAction;

        public ActionHandlerAsyncEnumerable(Action<Action<T1, T2>> registerAction, Action<Action<T1, T2>> disposeAction, CancellationToken cancellationToken)
        {
            this.cancellationToken1 = cancellationToken;
            this.registerAction = registerAction;
            this.disposeAction = disposeAction;
        }

        public IUniTaskAsyncEnumerator<Tuple<T1, T2>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this.cancellationToken1 == cancellationToken)
            {
                return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, CancellationToken.None);
            }
            return new ActionHandlerAsyncEnumerator(this.registerAction, this.disposeAction, this.cancellationToken1, cancellationToken);
        }

        class ActionHandlerAsyncEnumerator : MoveNextSource, IUniTaskAsyncEnumerator<Tuple<T1, T2>>
        {
            static readonly Action<object> cancel1 = OnCanceled1;
            static readonly Action<object> cancel2 = OnCanceled2;

            readonly Action<T1, T2> action;
            readonly CancellationToken cancellationToken1;
            readonly CancellationToken cancellationToken2;
            private readonly Action<Action<T1, T2>> disposeAction;
            private readonly Action<Action<T1, T2>> registerAction;
            bool isDisposed;

            bool isInitialized;
            CancellationTokenRegistration registration1;
            CancellationTokenRegistration registration2;

            public ActionHandlerAsyncEnumerator(Action<Action<T1, T2>> registerAction, Action<Action<T1, T2>> disposeAction, CancellationToken cancellationToken1, CancellationToken cancellationToken2)
            {
                this.action = Invoke;
                this.registerAction = registerAction;
                this.disposeAction = disposeAction;
                this.cancellationToken1 = cancellationToken1;
                this.cancellationToken2 = cancellationToken2;
            }

            public Tuple<T1, T2> Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                cancellationToken1.ThrowIfCancellationRequested();
                cancellationToken2.ThrowIfCancellationRequested();
                completionSource.Reset();

                if (!isInitialized)
                {
                    isInitialized = true;

                    TaskTracker.TrackActiveTask(this, 3);
                    this.registerAction(action);
                    if (cancellationToken1.CanBeCanceled)
                    {
                        registration1 = cancellationToken1.RegisterWithoutCaptureExecutionContext(cancel1, this);
                    }
                    if (cancellationToken2.CanBeCanceled)
                    {
                        registration2 = cancellationToken2.RegisterWithoutCaptureExecutionContext(cancel2, this);
                    }
                }

                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    TaskTracker.RemoveTracking(this);
                    registration1.Dispose();
                    registration2.Dispose();
                    if (action is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                    this.disposeAction(action);

                    completionSource.TrySetCanceled();
                }

                return default;
            }

            void Invoke(T1 value1, T2 value2)
            {
                Current = new Tuple<T1, T2>(value1, value2);
                completionSource.TrySetResult(true);
            }

            static void OnCanceled1(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken1);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }

            static void OnCanceled2(object state)
            {
                var self = (ActionHandlerAsyncEnumerator)state;
                try
                {
                    self.completionSource.TrySetCanceled(self.cancellationToken2);
                }
                finally
                {
                    self.DisposeAsync().Forget();
                }
            }
        }
    }
}
