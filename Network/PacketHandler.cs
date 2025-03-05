using DSMM.Network.Packets;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DSMM.Network
{
    public class PacketHandler
    {
        public static Dictionary<Type, PacketHandlerDelegate> Packets = new Dictionary<Type, PacketHandlerDelegate>();
        public static InputBuffer InputBuffer = new InputBuffer();

        public delegate void PacketHandlerDelegate(Player sender, object obj);

        public static byte[] SerializePacket(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static void DeserializePacket(CSteamID senderSteamID, byte[] data, uint dataSize)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                object deserializedObject = binaryFormatter.Deserialize(memoryStream);

                foreach (Type type in Packets.Keys)
                {
                    if (deserializedObject.GetType().Name == type.Name)
                    {
                        Packets.TryGetValue(type, out PacketHandlerDelegate action);

                        var packet = (Packet)deserializedObject;
                        if (packet.BufferPacket)
                        {
                            InputBuffer.AddToBuffer(senderSteamID, data);
                        }
                        else
                        {
                            Player player;

                            if (!NetworkManager.Instance.IsPlayer(senderSteamID.m_SteamID))
                            {
                                player = new Player(senderSteamID.m_SteamID);
                            }
                            else
                            {
                                player = NetworkManager.Instance.GetPlayer(senderSteamID.m_SteamID);
                            }

                            action.Invoke(player, deserializedObject);
                        }

                        return;
                    }
                }
            }
        }

        public static void ProcessBufferedPackets()
        {
            InputBuffer.ProcessBuffer();
        }
    }
}