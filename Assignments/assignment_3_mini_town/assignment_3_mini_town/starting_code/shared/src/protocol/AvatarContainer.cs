using System.Security.Cryptography;

namespace shared
{
    public class AvatarContainer : ISerializable
    {
        public int ID;
        public int SkinID;

        public float[] Position = new float[3];

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(SkinID);

            for (int i = 0; i < 3; i++)
            {
                pPacket.Write(Position[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            SkinID = pPacket.ReadInt();

            for (int i = 0; i < 3; i++)
            {
                Position[i] = pPacket.ReadFloat();
            }
        }
    }
}