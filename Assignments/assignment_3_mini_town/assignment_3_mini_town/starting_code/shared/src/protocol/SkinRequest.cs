using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class SkinRequest : ISerializable
    {
        public int ID;
        public int SkinID;

        public void Deserialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(SkinID);
        }

        public void Serialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            SkinID = pPacket.ReadInt();
        }
    }
}
