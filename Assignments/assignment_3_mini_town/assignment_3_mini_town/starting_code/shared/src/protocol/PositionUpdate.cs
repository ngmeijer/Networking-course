using shared.src;

namespace shared
{
    public class PositionUpdate : ISerializable
    {
        public int ID;
        public Vector3 Position = new Vector3();

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(Position);
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            Position = new Vector3();
        }
    }
}