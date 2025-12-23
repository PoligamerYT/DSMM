using DSMM.Network;
using MessagePack;
using MessagePack.Formatters;
using UnityEngine;

namespace DSMM.Formatters
{
    public class PlayerFormatter : IMessagePackFormatter<Player>
    {
        public void Serialize(ref MessagePackWriter writer, Player value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(5);

            writer.Write(value.SteamID);

            options.Resolver.GetFormatterWithVerify<Vector3>()
                .Serialize(ref writer, value.PlayerPosition, options);

            options.Resolver.GetFormatterWithVerify<Vector3>()
                .Serialize(ref writer, value.SwordPosition, options);

            writer.Write(value.SwordRotation);
            writer.Write(value.VelocityMagnitude);
        }

        public Player Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            int count = reader.ReadArrayHeader();

            ulong steamId = reader.ReadUInt64();

            var vectorFormatter = options.Resolver.GetFormatterWithVerify<Vector3>();

            Vector3 playerPos = vectorFormatter.Deserialize(ref reader, options);
            Vector3 swordPos = vectorFormatter.Deserialize(ref reader, options);

            float swordRot = reader.ReadSingle();
            float velocity = reader.ReadSingle();

            return new Player(steamId)
            {
                PlayerPosition = playerPos,
                SwordPosition = swordPos,
                SwordRotation = swordRot,
                VelocityMagnitude = velocity
            };
        }
    }
}
