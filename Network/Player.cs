using DSMM.Common;
using DSMM.Network.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;
using Vector3 = DSMM.Math.Vector3;

namespace DSMM.Network
{
    [Serializable]
    public class Player
    {
        public Player()
        {
        }

        public Player(ulong SteamID)
        {
            this.SteamID = SteamID;
        }

        public ulong SteamID { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public Vector3 SwordPosition { get; set; }
        public float SwordRotation { get; set; }
        public float VelocityMagnitude { get; set; }

        public PlayerController GetPlayerController()
        {
            return GameObject.Find(SteamID.ToString()).GetComponent<PlayerController>();
        }
    }
}
