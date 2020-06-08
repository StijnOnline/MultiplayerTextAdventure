using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class NewPlayerMessage : Message
    {
        public override MessageType Type => MessageType.NewPlayer;

        public int PlayerID { get; set; }
        public uint PlayerColor { get; set; }
        public string PlayerName { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(PlayerID);
            writer.WriteUInt(PlayerColor);
            writer.WriteString(PlayerName);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            PlayerID = reader.ReadInt();
            PlayerColor = reader.ReadUInt();
            PlayerName = reader.ReadString().ToString();
        }
    }
}
