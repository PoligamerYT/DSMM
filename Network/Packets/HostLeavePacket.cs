using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class HostLeavePacket : Packet
    {
        [Key(1)]
        public ulong HostSteamID { get; set; }
    }
}
