using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code {
    public class HitByMonserMessage : Message {
        public override MessageType Type => MessageType.HitByMonster;


        public int PlayerID { get; set; }
        public ushort newHP { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer) {
            base.SerializeObject(ref writer);
            writer.WriteUShort(newHP);
        }

        public override void DeserializeObject(ref DataStreamReader reader) {
            base.DeserializeObject(ref reader);
            newHP = reader.ReadUShort();
        }
    }
}
