using DSMM.Network.Enums;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class SwordChangePacket : Packet
    {
        public SwordChangeType SwordChangeType { get; set; }
    }
}
