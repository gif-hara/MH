using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MH
{
    public class DestroyFromSeconds : IGameObjectAction
    {
        [SerializeField]
        private float seconds;
        
        public async void Invoke(GameObject gameObject)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(this.seconds));
            
            Object.Destroy(gameObject);
        }
    }
}
