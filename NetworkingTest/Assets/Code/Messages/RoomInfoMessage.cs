using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class RoomInfoMessage : Message
    {
        public override MessageType Type => MessageType.RoomInfo;

        public Server.Game.Directions directions { get; set; }
        public ushort TreasureInRoom { get; set; }
        public bool ContainsMonster { get; set; }//bool
        public bool ContainsExit { get; set; }//bool
        public byte NumberofOtherPlayers { get; set; }
        public List<int> OtherPlayerIDs { get; set; } = new List<int>();

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteByte((byte) directions);
            writer.WriteUShort(TreasureInRoom);
            writer.WriteByte((byte)(ContainsMonster ? 1 : 0));
            writer.WriteByte((byte)(ContainsExit ? 1 : 0));
            writer.WriteByte(NumberofOtherPlayers);
            for(int i = 0; i < NumberofOtherPlayers; i++) {
                writer.WriteInt(OtherPlayerIDs[i]);
            }
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            directions = (Server.Game.Directions)reader.ReadByte();
            TreasureInRoom = reader.ReadUShort();
            ContainsMonster = reader.ReadByte()>0;
            ContainsExit = reader.ReadByte()>0;
            NumberofOtherPlayers = reader.ReadByte();
            OtherPlayerIDs.Clear(); // to be sure :)
            for(int i = 0; i < NumberofOtherPlayers; i++) {
                OtherPlayerIDs.Add(reader.ReadInt());
            }
        }

        
    }
}
