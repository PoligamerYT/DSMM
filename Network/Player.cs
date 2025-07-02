using System;
using UnityEngine;
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
