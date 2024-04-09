using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.src.protocol
{
    public class DeadAvatar : ISerializable
    {
        public int ID;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
        }
    }
}
