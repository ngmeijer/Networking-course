using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace shared.src.protocol
{
    public class HeartBeat : ISerializable
    {
        public int ClientID;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ClientID);
        }

        public void Deserialize(Packet pPacket)
        {
            ClientID = pPacket.ReadInt();
        }
    }
}
