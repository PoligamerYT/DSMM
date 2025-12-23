using MessagePack;
using UnityEngine;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PlayerPositionPacket : Packet
    {
        [Key(1)]
        public Vector3 PlayerPosition { get; set; }
        [Key(2)]
        public Vector3 SwordPosition { get; set; }
        [Key(3)]
        public float SwordRotation { get; set; }
        [Key(4)]
        public float MoveDirection { get; set; }
        [Key(5)]
        public float VelocityMagnitude { get; set; }
    }
}
