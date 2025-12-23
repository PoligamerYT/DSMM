using DSMM.Network.Enums;
using MessagePack;
using System.Collections.Generic;

namespace DSMM.Network.Packets
{
    [MessagePackObject]
    public class PrimaryInfoPacket : Packet
    {
        [Key(1)]
        public double Timestamp { get; set; }
        [Key(2)]
        public float PlayTime { get; set; }
        [Key(3)]
        public float TotalLapTime { get; set; }
        [Key(4)]
        public int LapCount { get; set; }
        [Key(5)]
        public int Stage { get; set; }
        [Key(6)]
        public int Sword { get; set; }
        [Key(7)]
        public List<Player> Players { get; set; }
        [Key(8)]
        public GameMode GameMode { get; set; }
        [Key(9)]
        public ControlType ControlType { get; set; }
    }
}
