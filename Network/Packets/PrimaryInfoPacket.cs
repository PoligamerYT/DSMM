using DSMM.Network.Enums;
using System;
using System.Collections.Generic;

namespace DSMM.Network.Packets
{
    [Serializable]
    public class PrimaryInfoPacket : Packet
    {
        public double Timestamp { get; set; }
        public float PlayTime { get; set; }
        public float TotalLapTime { get; set; }
        public int LapCount { get; set; }
        public int Stage { get; set; }
        public int Sword { get; set; }
        public List<Player> Players { get; set; }
        public GameMode GameMode { get; set; }
        public ControlType ControlType { get; set; }
    }
}
