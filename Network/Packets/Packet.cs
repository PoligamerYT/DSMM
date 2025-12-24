using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    [Union(0, typeof(CheckPointPacket))]
    [Union(1, typeof(HostLeavePacket))]
    [Union(2, typeof(PlayerActionPacket))]
    [Union(3, typeof(PlayerPositionPacket))]
    [Union(4, typeof(PlayerTeleportPacket))]
    [Union(5, typeof(PrimaryInfoPacket))]
    [Union(6, typeof(RestartGamePacket))]
    [Union(7, typeof(StageChangePacket))]
    [Union(8, typeof(SwordChangePacket))]
    [Union(9, typeof(PingPacket))]
    public class Packet
    {

    }
}