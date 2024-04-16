using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class Heartbeat : ASerializable
    {
        public string Status;

        public override void Deserialize(Packet pPacket)
        {
            Status = pPacket.ReadString();
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(Status);
        }
    }
}
