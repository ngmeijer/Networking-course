namespace shared
{
    public class AvatarContainer : ISerializable
    {
        public int ID;
        public int ClientID;
        public int SkinID;

        public float PosX;
        public float PosY;
        public float PosZ;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(SkinID);
            pPacket.Write(ClientID);

            pPacket.Write(PosX);
            pPacket.Write(PosY);
            pPacket.Write(PosZ);
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            SkinID = pPacket.ReadInt();
            ClientID = pPacket.ReadInt();

            PosX = pPacket.ReadFloat();
            PosY = pPacket.ReadFloat();
            PosZ = pPacket.ReadFloat();
        }
    }
}