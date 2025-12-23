using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class RestartGamePacket : Packet
    {
        [Key(1)]
        public float Timestamp { get; set; }
    }
}
