using Steamworks;
using System;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class HostLeavePacket : Packet
    {
        public CSteamID HostSteamID { get; set; }
    }
}
