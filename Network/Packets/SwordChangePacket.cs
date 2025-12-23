using DSMM.Network.Enums;
using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class SwordChangePacket : Packet
    {
        [Key(1)]
        public SwordChangeType SwordChangeType { get; set; }
    }
}
