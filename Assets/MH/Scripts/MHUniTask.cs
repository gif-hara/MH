using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public static class MHUniTask
    {
        public static IUniTaskAsyncEnumerable<AsyncUnit> AsyncActionHandler(
            Action<Action> registerAction,
            Action<Action> disposeAction,
            CancellationToken token
            )
        {
            return new ActionHandlerAsyncEnumerable(
                registerAction,
                disposeAction,
                token
                );
        }

        public static IUniTaskAsyncEnumerable<T> AsyncActionHandler<T>(
            Action<Action<T>> registerAction,
            Action<Action<T>> disposeAction,
            CancellationToken token
        )
        {
            return new ActionHandlerAsyncEnumerable<T>(
                registerAction,
                disposeAction,
                token
                );
        }

        public static IUniTaskAsyncEnumerable<Tuple<T1, T2>> AsyncActionHandler<T1, T2>(
            Action<Action<T1, T2>> registerAction,
            Action<Action<T1, T2>> disposeAction,
            CancellationToken token
        )
        {
            return new ActionHandlerAsyncEnumerable<T1, T2>(
                registerAction,
                disposeAction,
                token
                );
        }
    }
}
