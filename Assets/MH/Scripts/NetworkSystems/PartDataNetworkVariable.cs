using System;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// 部位データのネットワーク変数
    /// </summary>
    public struct PartDataNetworkVariable : INetworkSerializable, IEquatable<PartDataNetworkVariable>
    {
        public Define.PartType partType;

        public int endurance;

        public Vector3 opposePosition;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.partType);
            serializer.SerializeValue(ref this.endurance);
        }

        public bool Equals(PartDataNetworkVariable other)
        {
            return partType == other.partType && endurance == other.endurance && opposePosition.Equals(other.opposePosition);
        }

        public override bool Equals(object obj)
        {
            return obj is PartDataNetworkVariable other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)partType, endurance, opposePosition);
        }
    }
}
