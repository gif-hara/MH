using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// ステートコントローラー
    /// </summary>
    public sealed class StateController<T> : IDisposable where T : Enum
    {
        private class StateInfo
        {
            public Action<T, DisposableBagBuilder> onEnter;

            public Action<T> onExit;
        }

        private Dictionary<Enum, StateInfo> states = new();

        public T CurrentState { get; private set; }

        private T nextState;

        private T invalidState;

        /// <summary>
        /// ステート切り替え中であるか
        /// </summary>
        private bool isChanging;

        private readonly DisposableBagBuilder scope = DisposableBag.CreateBuilder();
        
        public StateController(T invalidState)
        {
            this.invalidState = invalidState;
            this.CurrentState = invalidState;
            this.nextState = invalidState;
        }

        public void Set(T value, Action<T, DisposableBagBuilder> onEnter, Action<T> onExit)
        {
            Assert.IsFalse(this.states.ContainsKey(value), $"{value}は既に登録済みです");

            this.states.Add(value, new StateInfo
            {
                onEnter = onEnter,
                onExit = onExit
            });
        }

        public async void ChangeRequest(T value)
        {
            if (this.isChanging)
            {
                return;
            }

            this.isChanging = true;
            this.nextState = value;
            await UniTask.NextFrame(PlayerLoopTiming.Update);
            this.isChanging = false;
            this.Change();
        }

        private void Change()
        {
            Assert.AreNotEqual(this.nextState, this.invalidState);
            if (this.states.ContainsKey(this.CurrentState))
            {
                this.states[this.CurrentState].onExit?.Invoke(this.nextState);
            }
            
            this.scope.Clear();
            var previousState = this.CurrentState;
            this.CurrentState = this.nextState;
            this.nextState = this.invalidState;

            if (this.states.ContainsKey(this.CurrentState))
            {
                this.states[this.CurrentState].onEnter?.Invoke(previousState, this.scope);
            }
        }

        public void Dispose()
        {
            this.scope.Clear();
        }
    }
}
