using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code {
    public class ReceivedMessage {
        public Message message { get; set; }
        public int connectionID { get; set; }

        public ReceivedMessage(Message message, int connectionID) {
            this.message = message;
            this.connectionID = connectionID;
        }
    }
}
