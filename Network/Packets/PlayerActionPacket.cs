using DSMM.Network.Enums;
using DSMM.Network.Packets;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class PlayerActionPacket : Packet
    {
        public PlayerActionType ActionType;
        public float ActionValue;
    }
}
