using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MH
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
            this.runtimeDataDictionary ??= this.dataList.ToDictionary(x => x.name, x => x.blendData);

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
