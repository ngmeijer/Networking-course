namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public int SenderID;
        public string Text;
        public float[] Position = new float[3];

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(SenderID);
            pPacket.Write(Text);

            for (int i = 0; i < 3; i++)
            {
                pPacket.Write(Position[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            SenderID = pPacket.ReadInt();
            Text = pPacket.ReadString();

            for (int i = 0; i < 3; i++)
            {
                Position[i] = pPacket.ReadFloat();
            }
        }
    }
}
