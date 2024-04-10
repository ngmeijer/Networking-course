using shared.src;
using System.Security.Cryptography;

namespace shared
{
    public class NewAvatar : ISerializable
    {
        public int ID;
        public int SkinID;
        public Vector3 Position;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(SkinID);
            pPacket.Write(Position);
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            SkinID = pPacket.ReadInt();
            Position = pPacket.ReadVector3();
        }
    }
}