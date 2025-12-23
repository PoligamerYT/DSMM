using DSMM.Network.Packets;
using MessagePack;
using Steamworks;
using System;
using System.Collections.Generic;

namespace DSMM.Network
{
    public class PacketHandler
    {
        public static Dictionary<Type, PacketHandlerDelegate> Packets = new Dictionary<Type, PacketHandlerDelegate>();

        public delegate void PacketHandlerDelegate(Player sender, object obj);

        public static byte[] SerializePacket<T>(T packet)
        {
            var unions = typeof(Packet).GetCustomAttributes(typeof(UnionAttribute), false);

            int id = -1;
            foreach (UnionAttribute union in unions)
            {
                if (union.SubType == packet.GetType())
                {
                    id = union.Key;
                    break;
                }
            }

            byte[] payload = MessagePackSerializer.Serialize(packet, NetworkManager.Instance.MessagePackOptions);

            byte[] data = new byte[payload.Length + 1];
            data[0] = (byte)id;
            Buffer.BlockCopy(payload, 0, data, 1, payload.Length);

            return data;
        }


        public static void DeserializePacket(CSteamID senderSteamID, byte[] data, uint dataSize)
        {
            int unionId = data[0];

            Type packetType = null;
            foreach (UnionAttribute union in typeof(Packet).GetCustomAttributes(typeof(UnionAttribute), false))
            {
                if (union.Key == unionId)
                {
                    packetType = union.SubType;
                    break;
                }
            }

            if (packetType == null)
                return;

            Packet packet = (Packet)MessagePackSerializer.Deserialize(packetType, data.AsSpan(1, (int)dataSize - 1).ToArray(), NetworkManager.Instance.MessagePackOptions);

            if (!Packets.TryGetValue(packetType, out var action))
                return;

            ulong steamId = senderSteamID.m_SteamID;

            Player player = NetworkManager.Instance.IsPlayer(steamId) ? NetworkManager.Instance.GetPlayer(steamId) : new Player(steamId);

            action.Invoke(player, packet);
        }
    }
}