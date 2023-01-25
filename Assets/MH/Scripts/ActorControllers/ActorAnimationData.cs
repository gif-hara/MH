using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.ActorControllers
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public sealed class ActorAnimationData
    {
        [SerializeField]
        private List<Data> dataList;

        private Dictionary<string, AnimationBlendData> runtimeDataDictionary;

        public AnimationBlendData GetAnimationBlendData(string name)
        {
#if UNITY_EDITOR
            this.runtimeDataDictionary = null;
#endif
            this.runtimeDataDictionary ??= this.dataList.ToDictionary(x => x.name, x => x.blendData);
            Assert.IsTrue(this.runtimeDataDictionary.ContainsKey(name), $"{name} is not found.");
            return this.runtimeDataDictionary[name];
        }

        [Serializable]
        public class Data
        {
            public string name;

            public AnimationBlendData blendData;
        }
    }
}
