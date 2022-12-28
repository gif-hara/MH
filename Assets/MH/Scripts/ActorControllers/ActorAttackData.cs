using System;
using System.Collections.Generic;

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
            public string motionName;
            
            public AnimationBlendData animationBlendData;

            /// <summary>
            /// 次に再生できるモーションの名前
            /// </summary>
            /// <remarks>
            /// 空文字の場合はそこでコンボ終了
            /// </remarks>
            public string nextMotionName;
        }
    }
}
