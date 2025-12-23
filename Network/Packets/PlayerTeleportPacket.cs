using MessagePack;
using UnityEngine;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PlayerTeleportPacket : Packet
    {
        [Key(1)]
        public Vector3 Position { get; set; }
    }
}
