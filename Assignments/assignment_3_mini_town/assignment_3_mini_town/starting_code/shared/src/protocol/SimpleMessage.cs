namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public int SenderID;
        public string Text;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(Text);
        }

        public void Deserialize(Packet pPacket)
        {
            Text = pPacket.ReadString();
        }
    }
}
