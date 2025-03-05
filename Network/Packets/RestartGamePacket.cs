using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class RestartGamePacket : Packet
    {
        public float Timestamp { get; set; }
    }
}
