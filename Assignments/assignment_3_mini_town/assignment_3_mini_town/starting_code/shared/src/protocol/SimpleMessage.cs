using shared.src;

namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public int SenderID;
        public string Text;
        public Vector3 Position = new Vector3();

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(SenderID);
            pPacket.Write(Text);
            pPacket.Write(Position);
        }

        public void Deserialize(Packet pPacket)
        {
            SenderID = pPacket.ReadInt();
            Text = pPacket.ReadString();
            Position = pPacket.ReadVector3();
        }
    }
}
