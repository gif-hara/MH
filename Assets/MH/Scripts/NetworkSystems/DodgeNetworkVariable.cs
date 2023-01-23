using DG.Tweening;
using MH.ActorControllers;
using Unity.Netcode;
using UnityEngine;

namespace MH.NetworkSystems
{
    /// <summary>
    /// ネットワーク上の回避に必要なデータ
    /// </summary>
    public struct DodgeNetworkVariable : INetworkSerializable
    {
        public Vector3 direction;

        public float duration;

        public Ease ease;

        public float speed;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref duration);
            serializer.SerializeValue(ref ease);
            serializer.SerializeValue(ref speed);
        }

        public ActorDodgeController.InvokeData ToInvokeData => new ()
        {
            direction = this.direction,
            duration = this.duration,
            ease = this.ease,
            speed = this.speed
        };
    }
}
