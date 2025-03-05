using DSMM.Math;
using Steamworks;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class PlayerPositionPacket : Packet
    {
        public Vector3 PlayerPosition { get; set; }
        public Vector3 SwordPosition { get; set; }
        public float SwordRotation { get; set; }
        public float MoveDirection { get; set; }
        public float VelocityMagnitude { get; set; }
    }
}
