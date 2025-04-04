using DSMM.Math;
using DSMM.Network.Enums;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class CheckPointPacket : Packet
    {
        public CheckPointMode Mode { get; set; }
        public Vector3 Location { get; set; }
    }
}
