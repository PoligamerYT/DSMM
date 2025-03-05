using DSMM.Network.Packets;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DSMM.Network
{
    public class InputBuffer
    {
        private struct BufferedPacket
        {
            public CSteamID SteamID;
            public byte[] PacketData;

            public BufferedPacket(CSteamID steamID, byte[] packetData)
            {
                SteamID = steamID;
                PacketData = packetData;
            }
        }

        private Queue<BufferedPacket> PacketQueue = new Queue<BufferedPacket>();

        public void AddToBuffer(CSteamID steamID, byte[] packetData)
        {
            PacketQueue.Enqueue(new BufferedPacket(steamID, packetData));
        }

        public void ProcessBuffer()
        {
            if(PacketQueue.Count > 0)
            {
                BufferedPacket bufferedPacket = PacketQueue.Dequeue();
                ProcessPacket(bufferedPacket.SteamID, bufferedPacket.PacketData);
            }
        }

        private void ProcessPacket(CSteamID remoteSteamID, byte[] packetData)
        {
            using (MemoryStream memoryStream = new MemoryStream(packetData))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                object deserializedObject = binaryFormatter.Deserialize(memoryStream);

                var packet = (Packet)deserializedObject;

                packet.BufferPacket = false;

                byte[] packetData_ = PacketHandler.SerializePacket(packet);

                PacketHandler.DeserializePacket(remoteSteamID, packetData_, (uint)packetData_.Length);
            }
        }
    }
}
