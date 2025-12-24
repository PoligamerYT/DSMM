using DSMM.Network.Enums;
using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PingPacket : Packet
    {
        [Key(1)]
        public double Timestamp { get; set; }
        [Key(2)]
        public SendType SendType { get; set; }
    }
}
