using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PingPacket : Packet
    {
        [Key(1)]
        public double Timestamp { get; set; }
    }
}
