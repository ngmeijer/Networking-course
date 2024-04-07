namespace shared
{
    public class PositionUpdate : ISerializable
    {
        public int ID;
        public float[] Position = new float[3];

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);

            for (int i = 0; i < 3; i++)
            {
                pPacket.Write(Position[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();

            for (int i = 0; i < 3; i++)
            {
                Position[i] = pPacket.ReadFloat();
            }
        }
    }
}