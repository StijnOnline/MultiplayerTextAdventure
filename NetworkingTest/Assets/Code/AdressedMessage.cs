namespace Assets.Code {
    public class AdressedMessage {
        public Message message { get; set; }
        public int connectionID { get; set; }

        public AdressedMessage(Message message, int connectionID) {
            this.message = message;
            this.connectionID = connectionID;
        }
    }
}
