using DSMM.Math;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class PlayerTeleportPacket : Packet
    {
        public Vector3 Position { get; set; }
    }
}
