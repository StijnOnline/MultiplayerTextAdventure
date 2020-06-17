using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code {
    public class EndGameMessage : Message {
        public override MessageType Type => MessageType.EndGame;

        public byte NumberOfScores;
        public List<ScoreData> scoreData;

        public class ScoreData {
            public int playerID;
            public ushort gold;
        }

        public override void SerializeObject(ref DataStreamWriter writer) {
            base.SerializeObject(ref writer);

            writer.WriteByte(NumberOfScores);
            foreach(ScoreData scoreData in scoreData) {
                writer.WriteInt(scoreData.playerID);
                writer.WriteUShort(scoreData.gold);
            }
        }

        public override void DeserializeObject(ref DataStreamReader reader) {
            base.DeserializeObject(ref reader);

            NumberOfScores = reader.ReadByte();
            scoreData.Clear();//to be sure
            for(int i = 0; i < NumberOfScores; i++) {
                scoreData.Add(new ScoreData {
                    playerID = reader.ReadInt(),
                    gold = reader.ReadUShort()
                }
                );
            }
        }
    }
}
