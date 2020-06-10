using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    /// <summary>
    /// Mainly used as a message to keep a connection alive 
    /// </summary>
    public class NoneMessage : Message
    {
        public override MessageType Type => MessageType.None;
    }
}
