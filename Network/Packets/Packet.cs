using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class Packet
    {
        public bool BufferPacket { get; set; } = false;
    }
}