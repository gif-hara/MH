using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の攻撃する際に必要なデータ
    /// </summary>
    [Serializable]
    public sealed class ActorAttackData
    {
        public List<MotionData> motionDataList;

        [Serializable]
        public class MotionData
        {
            public AnimationBlendData animationBlendData;

            /// <summary>
            /// 次に再生できるモーションのインデックス
            /// </summary>
            /// <remarks>
            /// <c>-1</c>の場合はそこでコンボ終了
            /// </remarks>
            public int nextMotionIndex;
        }
    }
}
