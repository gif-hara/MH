using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// 敵の次の行動を行うために必要なデータのネットワーク変数
    /// </summary>
    public struct ThinkDataNetworkVariable : INetworkSerializable
    {
        public Vector3 position;

        public float rotationY;

        public int thinkSeed;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.position);
            serializer.SerializeValue(ref this.rotationY);
            serializer.SerializeValue(ref this.thinkSeed);
        }
    }
}
