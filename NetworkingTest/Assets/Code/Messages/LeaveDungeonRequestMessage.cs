﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class LeaveDungeonRequestMessage : Message
    {
        public override MessageType Type => MessageType.LeaveDungeonRequest;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
        }
    }
}
