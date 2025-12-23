using DSMM.Network.Enums;
using MessagePack;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PlayerActionPacket : Packet
    {
        [Key(1)]
        public PlayerActionType ActionType;
        [Key(2)]
        public float ActionValue;
    }
}
