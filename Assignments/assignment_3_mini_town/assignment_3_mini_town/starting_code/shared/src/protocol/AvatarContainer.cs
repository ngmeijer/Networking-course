namespace shared
{
    public class AvatarContainer : ISerializable
    {
        public int ID;
        public int SkinID;
        public float[] Position;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(SkinID);
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            SkinID = pPacket.ReadInt();
        }
    }
}