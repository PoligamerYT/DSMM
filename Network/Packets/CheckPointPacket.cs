using DSMM.Network.Enums;
using MessagePack;
using UnityEngine;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class CheckPointPacket : Packet
    {
        [Key(1)]
        public CheckPointMode Mode { get; set; }
        [Key(2)]
        public Vector3 Location { get; set; }
    }
}
