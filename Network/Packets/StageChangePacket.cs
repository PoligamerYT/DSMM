using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class StageChangePacket : Packet
    {
        public int StageID { get; set; }
    }
}
