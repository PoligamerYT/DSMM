using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class StageChangePacket : Packet
    {
        [Key(1)]
        public int StageID { get; set; }
    }
}
